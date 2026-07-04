using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace HeadNonSub.Entities.Streamlabs.v6.Tip {

    public class Media {

        [JsonPropertyName("enabled")]
        public bool Enabled { get; set; }

        [JsonPropertyName("allowed_types")]
        public List<string> AllowedTypes { get; set; }

        [JsonPropertyName("min_amount_to_share")]
        public string MinAmountToShare { get; set; }

        [JsonPropertyName("price_per_second")]
        public string PricePerSecond { get; set; }

        [JsonPropertyName("max_duration")]
        public string MaxDuration { get; set; }

        [JsonPropertyName("volume")]
        public int Volume { get; set; }

        [JsonPropertyName("security")]
        public int Security { get; set; }

        [JsonPropertyName("auto_show_video")]
        public bool AutoShowVideo { get; set; }

        [JsonPropertyName("advanced_settings")]
        public AdvancedSettings AdvancedSettings { get; set; }

        [JsonPropertyName("tutorial_accordion_open")]
        public bool TutorialAccordionOpen { get; set; }

    }

}
