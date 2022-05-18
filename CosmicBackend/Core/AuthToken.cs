using System;
using Microsoft.IdentityModel.JsonWebTokens;
using Newtonsoft.Json.Linq;

namespace CosmicBackend.Core
{
    internal class AuthToken
    {
        internal AuthToken(JObject payload, int tokenExpirationTimeInMins)
        {
            JsonWebTokenHandler jwToken = new();
            jwToken.TokenLifetimeInMinutes = tokenExpirationTimeInMins;
            Token = jwToken.CreateToken(payload.ToString());
        }

        internal AuthToken() { }

        public string Token { get; protected set; }

        public TokenType TokenType { get; protected set; }

        public ulong TokenIdentifier { get; protected set; }

        public bool IsValid { get; protected set; } = true;

        public DateTime CreationDate { get; protected set; }

        public DateTime ExpirationDate { get; protected set; }

        internal bool HasExpired => DateTime.Now > ExpirationDate;

        internal JsonWebToken GetTokenObject(string token)
        {
            Token = token;
            try
            {
                JsonWebToken tokenObj = new(token);
                if (!tokenObj.TryGetPayloadValue("am", out TokenType tokenType) || tokenObj.TryGetPayloadValue("jti", out ulong seed))
                {
                    IsValid = false;
                }
                else
                {
                    TokenIdentifier = seed;
                }

                TokenType = tokenType;
                CreationDate = tokenObj.ValidFrom;
                ExpirationDate = tokenObj.ValidTo;
                IsValid = CreationDate != DateTime.MinValue && ExpirationDate != DateTime.MinValue;
                return tokenObj;
            }
            catch
            {
                IsValid = false;
                return null;
            }
        }
    }

    internal enum TokenType
    {
        exchange_code,
        client_credentials
    }
}