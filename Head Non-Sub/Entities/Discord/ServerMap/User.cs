using System;
using System.Text.Json.Serialization;

namespace HeadNonSub.Entities.Discord.ServerMap {

    public class User {

        [JsonPropertyName("id")]
        public ulong Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("created")]
        public DateTime Created { get; set; }

        [JsonPropertyName("joined")]
        public DateTime? Joined { get; set; }

        [JsonPropertyName("avatarUrl")]
        public string AvatarUrl { get; set; }

    }

}
