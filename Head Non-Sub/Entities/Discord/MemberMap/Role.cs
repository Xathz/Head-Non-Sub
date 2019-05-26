using Newtonsoft.Json;

namespace HeadNonSub.Entities.Discord.MemberMap {

    public class Role {

        [JsonProperty("id")]
        public ulong Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

    }

}
