using System.Text.Json.Serialization;

namespace HeadNonSub.Entities.Streamlabs.v6.Tip {

    public class AdvancedSettings {

        [JsonPropertyName("enabled")]
        public bool Enabled { get; set; }

        [JsonPropertyName("moderation_queue")]
        public bool ModerationQueue { get; set; }

        [JsonPropertyName("volume")]
        public int Volume { get; set; }

        [JsonPropertyName("auto_play")]
        public bool AutoPlay { get; set; }

        [JsonPropertyName("auto_show")]
        public bool AutoShow { get; set; }

        [JsonPropertyName("buffer_time")]
        public int BufferTime { get; set; }

        [JsonPropertyName("min_amount_to_share")]
        public string MinAmountToShare { get; set; }

        [JsonPropertyName("price_per_second")]
        public string PricePerSecond { get; set; }

        [JsonPropertyName("max_duration")]
        public string MaxDuration { get; set; }

        [JsonPropertyName("security")]
        public int Security { get; set; }

        [JsonPropertyName("requests_enabled")]
        public bool RequestsEnabled { get; set; }

        [JsonPropertyName("new_icon")]
        public bool NewIcon { get; set; }

        [JsonPropertyName("media_progress_bar")]
        public bool MediaProgressBar { get; set; }

        [JsonPropertyName("progress_bar_background_color")]
        public string ProgressBarBackgroundColor { get; set; }

        [JsonPropertyName("progress_bar_text_color")]
        public string ProgressBarTextColor { get; set; }

        [JsonPropertyName("backup_playlist_enabled")]
        public bool BackupPlaylistEnabled { get; set; }

    }

}
