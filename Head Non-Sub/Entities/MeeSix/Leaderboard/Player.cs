using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace HeadNonSub.Entities.MeeSix.Leaderboard {

    public class Player {

        [JsonPropertyName("avatar")]
        public string Avatar { get; set; }

        [JsonPropertyName("detailed_xp")]
        public List<int> DetailedXp { get; set; }

        [JsonPropertyName("discriminator")]
        public string Discriminator { get; set; }

        [JsonPropertyName("guild_id")]
        public string GuildId { get; set; }

        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("level")]
        public int Level { get; set; }

        [JsonPropertyName("username")]
        public string Username { get; set; }

        [JsonPropertyName("xp")]
        public int Xp { get; set; }

    }

}
