using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace HeadNonSub.Entities.Discord.MemberMap {

    public class MemberMap {

        [JsonPropertyName("members")]
        public List<Member> Members { get; set; } = new List<Member>();

    }

}
