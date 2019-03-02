using Newtonsoft.Json;

namespace HeadNonSub.Entities.Streamlabs {

    public class Polly {

        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("speak_url")]
        public string SpeakUrl { get; set; }

    }

}
