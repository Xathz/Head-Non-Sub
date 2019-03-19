using Newtonsoft.Json;

namespace HeadNonSub.Entities.Streamlabs.v6.Tip {

    public class Tip {

        [JsonProperty("settings")]
        public Settings Settings { get; set; }

    }

}
