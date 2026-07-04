using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace HeadNonSub.Entities.Xathz.Emotes {

    public class XathzEmotes {

        [JsonPropertyName("searchTerm")]
        public string SearchTerm { get; set; }

        [JsonPropertyName("executeDuration")]
        public string ExecuteDuration { get; set; }

        [JsonPropertyName("cacheStatus")]
        public string CacheStatus { get; set; }

        [JsonPropertyName("emoteCount")]
        public string EmoteCount { get; set; }

        [JsonPropertyName("emotes")]
        public List<Emote> Emotes { get; set; }

    }

}
