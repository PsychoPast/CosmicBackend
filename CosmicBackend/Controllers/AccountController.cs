using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CosmicBackend.Core;
using CosmicBackend.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;

namespace CosmicBackend.Controllers
{
    [Route("api/v1/oauth")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        [NoAuthorizationRequired]
        [HttpPost("register")]
        public async Task<IActionResult> RegisterHandler(
            [FromForm] string username,
            [FromForm] string email,
            [FromForm] string password)
        {
            MongoManager dbManager = Singleton.Instance.DatabaseManager;
            Regex requiredFormat = new("[0-9a-z]{1,25}", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            if (username == null || !requiredFormat.IsMatch(username))
            {
                return BadRequest(
                    Utilities.SetError("Wrong username format", "cosmic.register.username_wrong_format", 400).Value);
            }

            requiredFormat = new("([0-9a-z]{1,12})@cosmicfn.net", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            if (email == null || !requiredFormat.IsMatch(email))
            {
                return BadRequest(
                    Utilities.SetError("Wrong email format", "cosmic.register.email_wrong_format", 400).Value);
            }

            if (string.IsNullOrEmpty(password) || password.Length > 25)
            {
                return BadRequest(
                    Utilities.SetError("Wrong password format", "cosmic.register.password_wrong_format", 400).Value);
            }

            if (await dbManager.IsUsernameTakenAsync(username).ConfigureAwait(false))
            {
                return Conflict(
                    Utilities.SetError(
                        "Account with this username already exists",
                        "cosmic.register.username_taken",
                        409).Value);
            }

            if (!await dbManager.TryAddAccountAsync(
                     new User()
                         {
                             Username = username,
                             Credentials = new() { Email = email, Password = Utilities.HashPassword(password) }
                         }).ConfigureAwait(false))
            {
                return Conflict(
                    Utilities.SetError(
                        "Account with this email already exists",
                        "cosmic.register.email_already_in_use",
                        409).Value);
            }

            var status = new { statusCode = 200 };
            return Ok(new JsonResult(status).Value);
        }

        [NoAuthorizationRequired]
        [HttpPost("exchange")]
        public async Task<IActionResult> ExchangeHandler([FromForm] string username, [FromForm] string password)
        {
            MongoManager dbManager = Singleton.Instance.DatabaseManager;
            User user = await dbManager.GetAccount(username).ConfigureAwait(false);
            if (user == null)
            {
                return NotFound(
                    Utilities.SetError(
                        "Account with this username doesn't exist",
                        "cosmic.login.account_not_found",
                        404).Value);
            }

            if (Utilities.HashPassword(password) != user.Credentials.Password)
            {
                return Unauthorized(Utilities.SetError("Wrong password", "cosmic.login.wrong_password", 403).Value);
            }

            string userId = user.UserId;
            if (!Singleton.Instance.RegisteredSessions.TryGetValue(userId, out Session session))
            {
                session = new()
                              {
                                  Id = CityHash64.ComputeHash(username) ^ (ulong)Environment.TickCount64,
                                  PlayerUsername = username
                              };
                Singleton.Instance.RegisteredSessions[userId] = session;
            }

            var response = new
                               {
                                   code =
                                       $"{userId}{Utilities.ByteArrayToHexadecimalString(Utilities.ToByteArray(session.Id ^ ulong.MaxValue))}",
                                   statusCode = 200
                               };

            return Ok(new JsonResult(response).Value);
        }

        [NoAuthorizationRequired]
        [HttpPost("token")]
        public async Task<IActionResult> LoginHandler([FromForm] string code)
        {
            (string userId, ulong seed) = Utilities.DissectExchange(code);
            if (userId == null)
            {
                return BadRequest(
                    Utilities.SetError(
                        "Exchange code format is invalid",
                        "cosmic.login.exchange_code_wrong_format",
                        400).Value);
            }

            if (!Singleton.Instance.RegisteredSessions.TryGetValue(userId, out Session session))
            {
                return NotFound(
                    Utilities.SetError(
                        "Supplied exchange is unknown",
                        "cosmic.login.unknown_exchange_code",
                        404).Value);
            }

            if (session.Id != seed)
            {
                return Unauthorized(
                    Utilities.SetError(
                        "Supplied exchange doesn't match the user",
                        "cosmic.login.user_no_match_seed",
                        403).Value);
            }

            string username = session.PlayerUsername;
            var userInfos = new
                                {
                                    id = userId,
                                    username,
                                    token = new ExchangeCodeToken(userId, username, seed).Token,
                                    statusCode = 200
                                };

            return Ok(new JsonResult(userInfos).Value);
        }

        [HttpDelete("revokeToken")]
        public IActionResult RevokeTokenHandler([FromHeader] string authorization)
        {
            ExchangeCodeToken token = HttpContext.Items["authorization"] as ExchangeCodeToken;
            ulong seed = token.TokenIdentifier;
            string userId = token.UserId;
            ConcurrentDictionary<string, Session> sessions = Singleton.Instance.RegisteredSessions;
            if (!sessions.TryGetValue(userId, out Session session))
            {
                return NotFound(
                    Utilities.SetError("The token has already been revoked", "cosmic.jwt.token_already_revoked", 400)
                             .Value);
            }

            if (session.HasActiveFortniteSession)
            {
                return new ContentResult { StatusCode = 304 };
            }

            if (session.Id != seed)
            {
                return Unauthorized(
                    Utilities.SetError(
                        "The token doesn't belong to this account",
                        "cosmic.jwt.unauthorized_operation",
                        403).Value);
            }

            sessions.TryRemove(userId, out _);
            return Ok();
        }


        [HttpDelete("{accountId}")]
        public async Task<IActionResult> DeleteAccountHandler([FromRoute] string accountId)

        {
            ExchangeCodeToken token = HttpContext.Items["authorization"] as ExchangeCodeToken;
            if (!token.IsAdministrator)
            {
                return Unauthorized(
                    Utilities.SetError("Unauthorized Request", "cosmic.jwt.unauthorized_operation", 403).Value);
            }

            await Singleton.Instance.DatabaseManager.DeleteAccount(accountId).ConfigureAwait(false);
            return Ok();
        }

        [HttpDelete("/unregister/{accountId}")]
        public IActionResult ForceUnregisterHandler([FromRoute] string accountId)
        {
            ExchangeCodeToken token = HttpContext.Items["authorization"] as ExchangeCodeToken;
            if (!token.IsAdministrator)
            {
                return Unauthorized(
                    Utilities.SetError("Unauthorized Request", "cosmic.jwt.unauthorized_operation", 403).Value);
            }

            ConcurrentDictionary<string, Session> sessions = Singleton.Instance.RegisteredSessions;
            if (!sessions.TryRemove(accountId, out Session _))
            {
                return NotFound();
            }

            return Ok();
        }
    }
}