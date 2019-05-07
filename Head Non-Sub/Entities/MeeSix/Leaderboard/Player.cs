using System.Collections.Generic;
using Newtonsoft.Json;

namespace HeadNonSub.Entities.MeeSix.Leaderboard {

    public class Player {

        [JsonProperty("avatar")]
        public string Avatar { get; set; }

        [JsonProperty("detailed_xp")]
        public List<int> DetailedXp { get; set; }

        [JsonProperty("discriminator")]
        public string Discriminator { get; set; }

        [JsonProperty("guild_id")]
        public string GuildId { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("level")]
        public int Level { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("xp")]
        public int Xp { get; set; }

    }

}
