namespace CosmicBackend.Models
{
    internal class UserCredentials
    {
        internal string Email { get; set; }

        internal string Password { get; set; }
    }

    internal class User
    {
        internal string Username { get; set; }

        internal string UserId { get; set; }

        internal UserCredentials Credentials { get; set; }
    }

    internal class UserSkins
    {
        internal string UserId { get; set; }

        internal System.Collections.ObjectModel.Collection<string> SkinIdCollection { get; set; }
    }
}