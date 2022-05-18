namespace CosmicBackend.Core
{
    internal interface IToken
    { 
        internal AuthToken DeserializeToken(string token);
    }
}