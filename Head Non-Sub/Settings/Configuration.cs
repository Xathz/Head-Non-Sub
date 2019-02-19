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
        /// Delete the invoking message that ran the command.
        /// </summary>
        public bool DeleteInvokingMessage { get; set; } = false;

        /// <summary>
        /// List of users (user id's) to act as admins with commands. Key = server id; Value = hashset of user ids.
        /// </summary>
        public Dictionary<ulong, HashSet<ulong>> DiscordWhitelist { get; set; } = new Dictionary<ulong, HashSet<ulong>>();
        
    }

}
