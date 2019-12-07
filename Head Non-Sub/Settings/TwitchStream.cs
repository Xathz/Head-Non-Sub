using System.Collections.Generic;
using Newtonsoft.Json;

namespace HeadNonSub.Settings {

    public class TwitchStream {

        /// <summary>
        /// User id.
        /// </summary>
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// User id as a list for use in TwitchLib.
        /// </summary>
        [JsonIgnore]
        public List<string> UserIdAsList => new List<string>() { UserId };

        /// <summary>
        /// Username 'login name'.
        /// </summary>
        [JsonIgnore]
        public string Username => DisplayName.ToLowerInvariant();

        /// <summary>
        /// Username 'login name' as a list for use in TwitchLib.
        /// </summary>
        [JsonIgnore]
        public List<string> UsernameAsList => new List<string>() { Username };

        /// <summary>
        /// Formatted username.
        /// </summary>
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>
        /// Discord channel id to send the online/offline messages.
        /// </summary>
        public ulong DiscordChannel { get; set; } = 0;

        /// <summary>
        /// Url of the stream.
        /// </summary>
        [JsonIgnore]
        public string Url => $"https://www.twitch.tv/{Username}";

    }

}
