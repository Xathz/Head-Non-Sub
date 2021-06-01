using System.Collections.Generic;
using Newtonsoft.Json;

namespace HeadNonSub.Entities.Xathz.Emotes {

    public class XathzEmotes {

        [JsonProperty("searchTerm")]
        public string SearchTerm { get; set; }

        [JsonProperty("executeDuration")]
        public string ExecuteDuration { get; set; }

        [JsonProperty("cacheStatus")]
        public string CacheStatus { get; set; }

        [JsonProperty("emoteCount")]
        public string EmoteCount { get; set; }

        [JsonProperty("emotes")]
        public List<Emote> Emotes { get; set; }

    }

}
