using System.Text.Json.Serialization;

namespace HeadNonSub.Entities.MeeSix.Leaderboard {

    public class Role {

        [JsonPropertyName("color")]
        public int Color { get; set; }

        [JsonPropertyName("hoist")]
        public bool Hoist { get; set; }

        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("managed")]
        public bool Managed { get; set; }

        [JsonPropertyName("mentionable")]
        public bool Mentionable { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("permissions")]
        public int Permissions { get; set; }

        [JsonPropertyName("position")]
        public int Position { get; set; }

    }

}
