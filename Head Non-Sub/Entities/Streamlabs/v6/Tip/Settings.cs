using System;
using Newtonsoft.Json;

namespace HeadNonSub.Entities.Streamlabs.v6.Tip {

    public class Settings {

        [JsonIgnore]
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

        [JsonProperty("media")]
        public Media Media { get; set; }

    }

}
