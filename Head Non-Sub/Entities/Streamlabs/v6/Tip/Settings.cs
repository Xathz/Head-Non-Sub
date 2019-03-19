using Newtonsoft.Json;

namespace HeadNonSub.Entities.Streamlabs.v6.Tip {

    public class Settings {

        [JsonProperty("media")]
        public Media Media { get; set; }

    }

}
