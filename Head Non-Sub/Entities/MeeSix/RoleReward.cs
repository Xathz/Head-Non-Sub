using Newtonsoft.Json;

namespace HeadNonSub.Entities.MeeSix {

    public class RoleReward {

        [JsonProperty("rank")]
        public int Rank { get; set; }

        [JsonProperty("role")]
        public Role Role { get; set; }

    }

}
