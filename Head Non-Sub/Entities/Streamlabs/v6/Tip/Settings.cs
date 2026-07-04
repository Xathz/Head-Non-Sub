using System;
using System.Text.Json.Serialization;

namespace HeadNonSub.Entities.Streamlabs.v6.Tip {

    public class Settings {

        [JsonIgnore]
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

        [JsonPropertyName("media")]
        public Media Media { get; set; }

    }

}
