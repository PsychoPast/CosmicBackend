using System;
using System.Collections.Concurrent;
using CosmicBackend.Models;

namespace CosmicBackend.Core
{
    public class Singleton
    {
        internal MongoManager DatabaseManager = new();

        internal ConcurrentDictionary<string, Session> RegisteredSessions = new();

        internal ConcurrentDictionary<ulong, DateTime> RegisteredClientCredentialTokens = new();

        private static Singleton instance;

        public static Singleton Instance => instance ?? (instance = new());
    }
}