using System;
using Newtonsoft.Json;

namespace HeadNonSub.Entities.Discord.ServerMap {

    public class User {

        [JsonProperty("id")]
        public ulong Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("created")]
        public DateTime Created { get; set; }

        [JsonProperty("joined")]
        public DateTime? Joined { get; set; }

        [JsonProperty("avatarUrl")]
        public string AvatarUrl { get; set; }

    }

}
