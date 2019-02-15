using System;
using Newtonsoft.Json;

namespace HeadNonSub.Entities.Discord.ServerMap {

    public class Emote {

        [JsonProperty("id")]
        public ulong Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("created")]
        public DateTime Created { get; set; }

        [JsonProperty("creatorId")]
        public ulong CreatorId { get; set; }

        [JsonProperty("creatorName")]
        public string CreatorName { get; set; }

        [JsonProperty("animated")]
        public bool Animated { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

    }

}
