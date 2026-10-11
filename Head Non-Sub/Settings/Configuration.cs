using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

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
        [Required(ErrorMessage = "DiscordToken is required")]
        public string DiscordToken { get; set; } = string.Empty;

        /// <summary>
        /// Url shortener api key.
        /// </summary>
        public string UrlShortenerKey { get; set; } = string.Empty;

        /// <summary>
        /// MariaDB host.
        /// </summary>
        [Required(ErrorMessage = "MariaDBHost is required")]
        public string MariaDBHost { get; set; } = string.Empty;

        /// <summary>
        /// MariaDB database.
        /// </summary>
        [Required(ErrorMessage = "MariaDBDatabase is required")]
        public string MariaDBDatabase { get; set; } = string.Empty;

        /// <summary>
        /// MariaDB username.
        /// </summary>
        [Required(ErrorMessage = "MariaDBUsername is required")]
        public string MariaDBUsername { get; set; } = string.Empty;

        /// <summary>
        /// MariaDB password.
        /// </summary>
        [Required(ErrorMessage = "MariaDBPassword is required")]
        public string MariaDBPassword { get; set; } = string.Empty;

        /// <summary>
        /// Xathz's Emotes api key (https://emotes.xathz.net).
        /// </summary>
        public string XathzEmotesKey { get; set; } = string.Empty;

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
