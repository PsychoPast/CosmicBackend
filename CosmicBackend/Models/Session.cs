namespace CosmicBackend.Models
{
    using System;

    internal class Session
    {
        internal ulong Id { get; set; }

        internal string PlayerUsername { get; set; }

        internal bool HasActiveFortniteSession { get; set; }

        internal DateTime StartTime { get; set; }
    }
}