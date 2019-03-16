using Newtonsoft.Json;

namespace HeadNonSub.Entities.MeeSix {

    public class Guild {

        [JsonProperty("icon")]
        public string Icon { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("premium")]
        public bool Premium { get; set; }

    }

}
