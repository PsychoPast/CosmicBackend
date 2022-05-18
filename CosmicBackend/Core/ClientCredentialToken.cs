using System;
using Newtonsoft.Json.Linq;

namespace CosmicBackend.Core
{
    internal class ClientCredentialToken : AuthToken, IToken
    {
        internal ClientCredentialToken(ulong seed)
        :base(new JObject()
                  {
                      { "app", "cosmic" },
                      { "am" , TokenType.client_credentials.ToString()},
                      { "jti", seed }
                  }, 240)
        {
            TokenIdentifier = seed;
            Singleton.Instance.RegisteredClientCredentialTokens.GetOrAdd(seed, DateTime.Now);
        }

        AuthToken IToken.DeserializeToken(string token)
        {
            GetTokenObject(token);
            return this;
        }
    }
}