using System.Collections.Generic;
using Newtonsoft.Json;

namespace HeadNonSub.Entities.MeeSix.Moderator {

    public class Moderator {

        [JsonProperty("infractions")]
        public List<Infraction> Infractions { get; set; }

        [JsonProperty("page")]
        public int Page { get; set; }

    }

}
