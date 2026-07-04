using System.Text.Json.Serialization;

namespace HeadNonSub.Entities.Streamlabs {

    public class Polly {

        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("speak_url")]
        public string SpeakUrl { get; set; }

    }

}
