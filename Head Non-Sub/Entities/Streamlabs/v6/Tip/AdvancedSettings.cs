using Newtonsoft.Json;

namespace HeadNonSub.Entities.Streamlabs.v6.Tip {

    public class AdvancedSettings {

        [JsonProperty("enabled")]
        public bool Enabled { get; set; }

        [JsonProperty("moderation_queue")]
        public bool ModerationQueue { get; set; }

        [JsonProperty("volume")]
        public int Volume { get; set; }

        [JsonProperty("auto_play")]
        public bool AutoPlay { get; set; }

        [JsonProperty("auto_show")]
        public bool AutoShow { get; set; }

        [JsonProperty("buffer_time")]
        public int BufferTime { get; set; }

        [JsonProperty("min_amount_to_share")]
        public string MinAmountToShare { get; set; }

        [JsonProperty("price_per_second")]
        public string PricePerSecond { get; set; }

        [JsonProperty("max_duration")]
        public string MaxDuration { get; set; }

        [JsonProperty("security")]
        public int Security { get; set; }

        [JsonProperty("requests_enabled")]
        public bool RequestsEnabled { get; set; }

        [JsonProperty("new_icon")]
        public bool NewIcon { get; set; }

        [JsonProperty("media_progress_bar")]
        public bool MediaProgressBar { get; set; }

        [JsonProperty("progress_bar_background_color")]
        public string ProgressBarBackgroundColor { get; set; }

        [JsonProperty("progress_bar_text_color")]
        public string ProgressBarTextColor { get; set; }

        [JsonProperty("backup_playlist_enabled")]
        public bool BackupPlaylistEnabled { get; set; }

    }

}
