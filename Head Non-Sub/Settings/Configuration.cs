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
        /// List of users (user id's) to act as admins with commands. Key = server id; Value = hashset of user ids.
        /// </summary>
        public Dictionary<ulong, HashSet<ulong>> DiscordWhitelist { get; set; } = new Dictionary<ulong, HashSet<ulong>>();

        /// <summary>
        /// List of users (user id's) that are blacklisted from using any commands. Key = server id; Value = hashset of user ids.
        /// </summary>
        public Dictionary<ulong, HashSet<ulong>> DiscordBlacklist { get; set; } = new Dictionary<ulong, HashSet<ulong>>();

    }

}
