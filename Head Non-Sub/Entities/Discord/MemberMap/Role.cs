using System.Text.Json.Serialization;

namespace HeadNonSub.Entities.Discord.MemberMap {

    public class Role {

        [JsonPropertyName("id")]
        public ulong Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

    }

}
