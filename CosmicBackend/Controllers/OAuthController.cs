using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using CosmicBackend.Core;
using CosmicBackend.Models;
using Microsoft.AspNetCore.Mvc;

namespace CosmicBackend.Controllers
{
    [Route("/accounts/api/oauth")]
    [ApiController]

    public class OAuthController : ControllerBase
    {
        [HttpPost("token")]
        public async Task<IActionResult> OAuthHandler([FromForm] string grant_type, [FromForm] string exchange_code)
        {
            switch(grant_type)
            {
                case "exchange_code":
                    {
                        (string userId, ulong seed) = Utilities.DissectExchange(exchange_code);
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
                            return NotFound(Utilities.SetError("Supplied exchange is unknown", "cosmic.login.unknown_exchange", 404).Value);
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
                        var response = new
                        {
                            access_token = $"eg1~{new ExchangeCodeToken(userId, username, seed).Token}",
                            account_id = userId,
                            app = "fortnite",
                            client_id = Globals.ClientId,
                            client_service = "fortnite",
                            device_id = Globals.DeviceId,
                            displayName = username,
                            expires_at = DateTime.Now.AddYears(90),
                            expires_in = 999999999,
                            in_app_id = userId,
                            internal_client = true,
                            refresh_expires = 999999999,
                            refresh_expires_at = DateTime.Now.AddYears(90),
                            refresh_token = Globals.RefreshToken,
                            token_type = "bearer"
                        };
                        session.HasActiveFortniteSession = true;
                        return Ok(new JsonResult(response).Value);
                    }

                case "client_credentials":
                    {
                        var response = new
                        {
                            access_token = $"eg1~{new ClientCredentialToken((ulong)Environment.TickCount64)}",
                            client_id = Globals.ClientId,
                            client_service = "fortnite",
                            expires_at = DateTime.Now.AddYears(90),
                            expires_in = 999999999,
                            internal_client = true,
                            token_type = "bearer"
                        };
                        return Ok(new JsonResult(response).Value);
                    }
            }

            return BadRequest(Utilities.SetError("Unknown grant type", "cosmic.fn.oauth.unknown_grant_type", 400).Value);
        }

        [HttpDelete("/sessions/kill/{token}")]
        public IActionResult KillSessionHandler([FromHeader] string authorization, [FromRoute] string token)
        {
            if (authorization != token)
            {
                return Unauthorized(
                    Utilities.SetError(
                        "Authorization token doesn't match target one",
                        "cosmic.fn.oauth.diff_token",
                        403).Value);
            }

            IToken tok = new ExchangeCodeToken();
            ExchangeCodeToken tokenObj = (ExchangeCodeToken)tok.DeserializeToken(token);
            ulong seed = tokenObj.TokenIdentifier;
            string userId = tokenObj.UserId;
            ConcurrentDictionary<string, Session> sessions = Singleton.Instance.RegisteredSessions;
            if (!sessions.TryGetValue(userId, out Session session))
            {
                return NotFound(
                    Utilities.SetError("The token has already been revoked", "cosmic.jwt.token_already_revoked", 400)
                             .Value);
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
    }
}