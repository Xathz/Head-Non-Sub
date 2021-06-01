using Newtonsoft.Json;

namespace HeadNonSub.Entities.Xathz.Emotes {

    public class Emote {

        [JsonProperty("xxHash64")]
        public string XXHash64 { get; set; }

        [JsonProperty("size")]
        public string Size { get; set; }

        [JsonProperty("hashDuplicateCount")]
        public int HashDuplicateCount { get; set; }

        [JsonProperty("distinctNameCount")]
        public int DistinctNameCount { get; set; }

        [JsonProperty("names")]
        public string Names { get; set; }

        [JsonProperty("file")]
        public string File { get; set; }

    }

}
