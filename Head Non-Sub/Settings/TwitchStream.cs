using Newtonsoft.Json;

namespace HeadNonSub.Settings {

    public class TwitchStream {

        public string Username { get; set; }

        [JsonIgnore]
        public string UsernameLowercase => Username.ToLower();

        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// Formatted username.
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Discord channel id to send the online/offline messages.
        /// </summary>
        public ulong DiscordChannel { get; set; }

        /// <summary>
        /// Url of the stream.
        /// </summary>
        public string StreamUrl { get; set; }

    }

}
