using System;
using System.Text.Json.Serialization;

namespace HeadNonSub.Entities.Discord.ServerMap {

    public class Emote {

        [JsonPropertyName("id")]
        public ulong Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("created")]
        public DateTime Created { get; set; }

        [JsonPropertyName("creatorId")]
        public ulong CreatorId { get; set; }

        [JsonPropertyName("creatorName")]
        public string CreatorName { get; set; }

        [JsonPropertyName("animated")]
        public bool Animated { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; }

    }

}
