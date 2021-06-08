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
        /// Twitch client id.
        /// </summary>
        public string TwitchClientId { get; set; } = string.Empty;

        /// <summary>
        /// Twitch token.
        /// </summary>
        public string TwitchToken { get; set; } = string.Empty;

        /// <summary>
        /// Twitch refresh token.
        /// </summary>
        public string TwitchRefresh { get; set; } = string.Empty;

        /// <summary>
        /// MeeSix (MEE6) api token.
        /// </summary>
        public string MeeSixToken { get; set; } = string.Empty;

        /// <summary>
        /// CBenni api token.
        /// </summary>
        public string CBenniToken { get; set; } = string.Empty;

        /// <summary>
        /// Url shortener api key.
        /// </summary>
        public string UrlShortenerKey { get; set; } = string.Empty;

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
        /// Xathz's Emotes api key (https://emotes.xathz.net).
        /// </summary>
        public string XathzEmotesKey { get; set; } = string.Empty;

        /// <summary>
        /// Backblaze temporary files bucket.
        /// </summary>
        public BackblazeBucket BackblazeTempBucket { get; set; } = new BackblazeBucket();

        /// <summary>
        /// Backblaze avatar files bucket.
        /// </summary>
        public BackblazeBucket BackblazeAvatarBucket { get; set; } = new BackblazeBucket();

        /// <summary>
        /// Twitch stream to monitor and report online/offline status.
        /// </summary>
        public TwitchStream TwitchStream { get; set; } = new TwitchStream();

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
