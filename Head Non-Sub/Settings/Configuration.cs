using System.Collections.Generic;

namespace HeadNonSub.Settings {

    public class Configuration {

        /// <summary>
        /// Bot nickname.
        /// </summary>
        public string BotNickname { get; set; } = "Head Non-Sub";

        /// <summary>
        /// What "game" is the bot playing.
        /// </summary>
        public string BotPlaying { get; set; } = "pls gift me a sub";

        /// <summary>
        /// Discord api token.
        /// </summary>
        public string DiscordToken { get; set; } = string.Empty;

        /// <summary>
        /// Twitch username.
        /// </summary>
        public string TwitchUsername { get; set; } = string.Empty;

        /// <summary>
        /// Twitch oauth token.
        /// </summary>
        public string TwitchToken { get; set; } = string.Empty;

        /// <summary>
        /// MeeSix (MEE6) api token.
        /// </summary>
        public string MeeSixToken { get; set; } = string.Empty;

        /// <summary>
        /// CBenni api token.
        /// </summary>
        public string CBenniToken { get; set; } = string.Empty;

        /// <summary>
        /// Key to upload files to 'upload.php'. Check project repository.
        /// </summary>
        public string CDNKey { get; set; } = string.Empty;

        /// <summary>
        /// MariaDB host.
        /// </summary>
        public string MariaDBHost { get; set; } = string.Empty;

        /// <summary>
        /// MariaDB database.
        /// </summary>
        public string MariaDBDatabase { get; set; } = string.Empty;

        /// <summary>
        /// MariaDB username.
        /// </summary>
        public string MariaDBUsername { get; set; } = string.Empty;

        /// <summary>
        /// MariaDB password.
        /// </summary>
        public string MariaDBPassword { get; set; } = string.Empty;

        /// <summary>
        /// List of Twitch streams to monitor and report online/offline status.
        /// </summary>
        public List<TwitchStream> TwitchStreams { get; set; } = new List<TwitchStream>();

        /// <summary>
        /// List of users (user id's) to act as admins with commands. Key = server id; Value = hashset of user ids.
        /// </summary>
        public Dictionary<ulong, HashSet<ulong>> DiscordWhitelist { get; set; } = new Dictionary<ulong, HashSet<ulong>>();

        /// <summary>
        /// List of users (user id's) that are blacklisted from using any commands. Key = server id; Value = hashset of user ids.
        /// </summary>
        public Dictionary<ulong, HashSet<ulong>> DiscordBlacklist { get; set; } = new Dictionary<ulong, HashSet<ulong>>();

    }

}
