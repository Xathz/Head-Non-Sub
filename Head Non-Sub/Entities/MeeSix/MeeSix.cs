using System.Collections.Generic;
using Newtonsoft.Json;

namespace HeadNonSub.Entities.MeeSix {

    public class MeeSix {

        [JsonProperty("guild")]
        public Guild Guild { get; set; }

        [JsonProperty("players")]
        public List<Player> Players { get; set; }

        [JsonProperty("role_rewards")]
        public List<RoleReward> RoleRewards { get; set; }

    }

}
