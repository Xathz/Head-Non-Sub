using System.Text.Json.Serialization;

namespace HeadNonSub.Entities.MeeSix.Leaderboard {

    public class RoleReward {

        [JsonPropertyName("rank")]
        public int Rank { get; set; }

        [JsonPropertyName("role")]
        public Role Role { get; set; }

    }

}
