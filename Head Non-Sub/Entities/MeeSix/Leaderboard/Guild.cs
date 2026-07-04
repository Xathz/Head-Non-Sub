using System.Text.Json.Serialization;

namespace HeadNonSub.Entities.MeeSix.Leaderboard {

    public class Guild {

        [JsonPropertyName("icon")]
        public string Icon { get; set; }

        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("premium")]
        public bool Premium { get; set; }

    }

}
