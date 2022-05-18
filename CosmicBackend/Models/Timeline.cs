using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CosmicBackend.Models
{
    internal class Timeline
    {
        [JsonPropertyName("channels")]
        internal Channels Channels { get; set; }

        [JsonPropertyName("cacheIntervalMins")]
        internal uint CacheInterval { get ; set; }

        [JsonPropertyName("currentTime")]
        internal DateTime CurrentTime { get; set; }
    }

    internal class Channels
    {
        [JsonPropertyName("standalone-store")]
        internal object StandaloneStore { get; set; }

        [JsonPropertyName("client-matchmaking")]
        internal object ClientMatchmaking { get; set; }

        [JsonPropertyName("tk")]
        internal object Tk { get; set; }

        [JsonPropertyName("community-votes")]
        internal object CommunityVotes { get; set; }

        [JsonPropertyName("featured-islands")]
        internal object FeaturedIslands { get; set; }

        [JsonPropertyName("client-events")]
        internal ClientEvents ClientEvents { get; set; }
    }

    internal class ClientEvents
    {
        [JsonPropertyName("cacheExpire")]
        internal DateTime CacheExpire { get; set; }

        [JsonPropertyName("states")]
        internal List<ClientEventState> ClientEventStates { get; set; }
    }

    internal class ClientEventState
    {
        [JsonPropertyName("validFrom")]
        internal DateTime ValidFrom { get; set; }

        //[JsonPropertyName("states")]
    }
}