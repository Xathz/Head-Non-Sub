using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace HeadNonSub.Entities.MeeSix.Moderator {

    public class Moderator {

        [JsonPropertyName("infractions")]
        public List<Infraction> Infractions { get; set; }

        [JsonPropertyName("page")]
        public int Page { get; set; }

    }

}
