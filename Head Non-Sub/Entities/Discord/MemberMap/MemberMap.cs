using System.Collections.Generic;
using Newtonsoft.Json;

namespace HeadNonSub.Entities.Discord.MemberMap {

    public class MemberMap {

        [JsonProperty("members")]
        public List<Member> Members { get; set; } = new List<Member>();

    }

}
