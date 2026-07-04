using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace HeadNonSub.Entities.MeeSix.Leaderboard {

    public class Leaderboard {

        [JsonPropertyName("guild")]
        public Guild Guild { get; set; }

        [JsonPropertyName("players")]
        public List<Player> Players { get; set; }

        [JsonPropertyName("role_rewards")]
        public List<RoleReward> RoleRewards { get; set; }

    }

}
