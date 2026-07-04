using System.Text.Json.Serialization;

namespace HeadNonSub.Entities.Streamlabs.v6.Tip {

    public class Tip {

        [JsonPropertyName("settings")]
        public Settings Settings { get; set; }

    }

}
