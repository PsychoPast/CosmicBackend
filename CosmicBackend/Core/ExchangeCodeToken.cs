using System;
using Microsoft.IdentityModel.JsonWebTokens;
using Newtonsoft.Json.Linq;

namespace CosmicBackend.Core
{
    internal class ExchangeCodeToken : AuthToken, IToken
    {
        internal ExchangeCodeToken(string id, string username, ulong seed)
            : this(id, username, username.StartsWith('[') ? Globals.AdministratorFlag : 0, seed)
        {
        }

        internal ExchangeCodeToken() { }

        private ExchangeCodeToken(string id, string username, int accessType, ulong seed)
            : base(new JObject()
                       {
                           { "app", "cosmic" },
                           { "sub",  id },
                           { "dn", username },
                           { "am", TokenType.exchange_code.ToString() },
                           { "p", accessType },
                           { "jti", seed }
                       }, 480)
        {
            IsAdministrator = accessType != 0;
            UserId = id;
            Username = username;
            TokenIdentifier = seed;
            CreationDate = DateTime.Now;
            ExpirationDate = CreationDate.AddHours(8);
        }

        internal bool IsAdministrator { get; private set; }

        internal string UserId { get; private set; }

        internal string Username { get; private set; }

        AuthToken IToken.DeserializeToken(string token)
        {
            JsonWebToken jwToken = GetTokenObject(token);
            if (jwToken == null || !IsValid
                                || !jwToken.TryGetPayloadValue("sub", out string userId)
                                || !jwToken.TryGetPayloadValue("dn", out string username)
                                || !jwToken.TryGetPayloadValue("p", out int accessFlag))
            {
                return this;
            }

            IsAdministrator = accessFlag != 0;
            UserId = userId;
            Username = username;
            return this;
        }
    }
}