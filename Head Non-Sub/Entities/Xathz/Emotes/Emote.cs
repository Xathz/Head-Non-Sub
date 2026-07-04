using System.Text.Json.Serialization;

namespace HeadNonSub.Entities.Xathz.Emotes {

    public class Emote {

        [JsonPropertyName("xxHash64")]
        public string XXHash64 { get; set; }

        [JsonPropertyName("size")]
        public string Size { get; set; }

        [JsonPropertyName("hashDuplicateCount")]
        public int HashDuplicateCount { get; set; }

        [JsonPropertyName("distinctNameCount")]
        public int DistinctNameCount { get; set; }

        [JsonPropertyName("names")]
        public string Names { get; set; }

        [JsonPropertyName("file")]
        public string File { get; set; }

    }

}
