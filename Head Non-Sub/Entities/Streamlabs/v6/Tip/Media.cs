using System.Collections.Generic;
using Newtonsoft.Json;

namespace HeadNonSub.Entities.Streamlabs.v6.Tip {

    public class Media {

        [JsonProperty("enabled")]
        public bool Enabled { get; set; }

        [JsonProperty("allowed_types")]
        public List<string> AllowedTypes { get; set; }

        [JsonProperty("min_amount_to_share")]
        public string MinAmountToShare { get; set; }

        [JsonProperty("price_per_second")]
        public string PricePerSecond { get; set; }

        [JsonProperty("max_duration")]
        public string MaxDuration { get; set; }

        [JsonProperty("volume")]
        public int Volume { get; set; }

        [JsonProperty("security")]
        public int Security { get; set; }

        [JsonProperty("auto_show_video")]
        public bool AutoShowVideo { get; set; }

        [JsonProperty("advanced_settings")]
        public AdvancedSettings AdvancedSettings { get; set; }

        [JsonProperty("tutorial_accordion_open")]
        public bool TutorialAccordionOpen { get; set; }

    }

}
