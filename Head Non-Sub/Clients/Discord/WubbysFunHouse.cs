using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Discord;
using Discord.WebSocket;

namespace HeadNonSub.Clients.Discord {

    /// <summary>
    /// Wubby's Fun House
    /// </summary>
    public static class WubbysFunHouse {

        public const ulong ServerId = 328300333010911242;

        #region Urls

        public const string IconUrl = "https://cdn.discordapp.com/attachments/559869208976949278/602617867355160667/PaymoneyWubbyLogo.jpg";

        public const string WubbyMoneyEmoteUrl = "https://cdn.discordapp.com/emojis/522586653114499072.png";

        public const string TwitchStocksUrl = "https://twitchstocks.com/stock/pymny";

        public const string TwitchSubscribeUrl = "https://wub.by/subttv";

        public const string LinkTwitchToDiscordUrl = "https://wub.by/linkttv";

        #endregion

        #region Channel ids

        public const ulong MainChannelId = 403341336129830918;

        public const ulong LinksChannelId = 553407177654665216;

        public const ulong ActualFuckingSpamChannelId = 537727672747294738;

        public const ulong ModLogsChannelId = 502940074036690954;

        public const ulong UserLogsChannelId = 490766846761500683;

        public const ulong ModsChannelId = 490766924201066516;

        #endregion

        #region Channel category ids

        public const ulong ModDaddiesCategoryId = 490764927448121345;

        #endregion

        #region User ids

        public const ulong PaymoneyWubbyUserId = 177657233025400832;

        #endregion

        #region Bot ids

        public const ulong HeadNonSubBotUserId = 545778771517636638;

        public const ulong MediaShareBotUserId = 511449458819596289;

        public const ulong MEE6BotUserId = 159985870458322944;

        public const ulong PollBotUserId = 266162291735658496;

        public const ulong RythmBotUserId = 235088799074484224;

        public const ulong Rythm2BotUserId = 252128902418268161;

        public const ulong StatbotBotUserId = 491769129318088714;

        public const ulong WubbyBotUserId = 489538588120449039;

        public const ulong RawgoatBotUserId = 348350443031625738;

        public const ulong WubbyIRLBotUserId = 529090224324608000;

        // TODO Re-enable, bot is having issues currently 2019-11-24T20:37:40+0000
        //public const ulong WubbyMailBotUserId = 523299238369820692;
        //public static readonly ReadOnlyCollection<ulong> AllBotIds = new ReadOnlyCollection<ulong>(new List<ulong> { HeadNonSubBotUserId, MediaShareBotUserId, MEE6BotUserId, PollBotUserId, RythmBotUserId, Rythm2BotUserId, StatbotBotUserId, WubbyBotUserId, RawgoatBotUserId, WubbyIRLBotUserId, WubbyMailBotUserId, LogBotUserId });

        public const ulong LogBotUserId = 555800636332179480;

        /// <summary>
        /// All bot user ids.
        /// </summary>
        public static readonly ReadOnlyCollection<ulong> AllBotIds = new ReadOnlyCollection<ulong>(new List<ulong> { HeadNonSubBotUserId, MediaShareBotUserId, MEE6BotUserId, PollBotUserId, RythmBotUserId, Rythm2BotUserId, StatbotBotUserId, WubbyBotUserId, RawgoatBotUserId, WubbyIRLBotUserId, LogBotUserId });

        #endregion

        #region Role ids

        public const ulong GingerBoyRoleId = 465872398772862976;

        public const ulong AdminsRoleId = 372244721625464845;

        public const ulong ModsRoleId = 336022934621519874;

        public const ulong ModLiteRoleId = 497550793923362827;

        public const ulong TwitchModRoleId = 468960777978642443;

        public const ulong SubredditModRoleId = 542135412106330122;

        public const ulong BotsRoleId = 328403426142715906;

        public const ulong TwitchSubscriberRoleId = 428052879371272192;

        public const ulong PatronRoleId = 328732005024137217;

        public const ulong NonSubRoleId = 644457029406162964;

        public const ulong MutedRoleId = 445807715655221259;

        public const ulong Tier3RoleId = 493641643765792768;

        public const ulong RuleReaderRoleId = 580577184809353226;

        /// <summary>MEE6 rank 10</summary>
        /// <remarks>
        /// https://wub.by/discordranks
        /// https://mee6.xyz/leaderboard/328300333010911242
        /// </remarks>
        public const ulong ForkliftDriversRoleId = 502698826830708737;

        /// <summary>
        /// Admins, mods, and mod-lites.
        /// </summary>
        public static readonly ReadOnlyCollection<ulong> DiscordStaffRoles = new ReadOnlyCollection<ulong>(new List<ulong> { GingerBoyRoleId, AdminsRoleId, ModsRoleId, ModLiteRoleId });

        /// <summary>
        /// Admins, mods, mod-lites, and twitch mods.
        /// </summary>
        public static readonly ReadOnlyCollection<ulong> TwitchStaffRoles = new ReadOnlyCollection<ulong>(new List<ulong> { GingerBoyRoleId, AdminsRoleId, ModsRoleId, ModLiteRoleId, TwitchModRoleId });

        /// <summary>
        /// Admins, mods, mod-lites, twitch mods, and subreddit mods.
        /// </summary>
        public static readonly ReadOnlyCollection<ulong> AllStaffRoles = new ReadOnlyCollection<ulong>(new List<ulong> { GingerBoyRoleId, AdminsRoleId, ModsRoleId, ModLiteRoleId, TwitchModRoleId, SubredditModRoleId });

        #endregion

        /// <summary>
        /// Get if the user id is a server bot.
        /// </summary>
        public static bool IsServerBot(ulong id) => AllBotIds.Contains(id);

        /// <summary>
        /// Get if the user is a discord staff member.
        /// </summary>
        public static bool IsDiscordStaff(IUser user) {
            if (user is SocketGuildUser socketUser) {

                // PaymoneyWubby
                if (socketUser.Id == PaymoneyWubbyUserId) {
                    return true;
                }

                // Xathz
                if (socketUser.Id == Constants.XathzUserId) {
                    return true;
                }

                // Administrator
                if (socketUser.Roles.Any(x => x.Permissions.Administrator)) {
                    return true;
                }

                // Discord staff
                if (socketUser.Roles.Any(x => DiscordStaffRoles.Contains(x.Id))) {
                    return true;
                }

                return false;
            } else {
                return false;
            }
        }

        /// <summary>
        /// Get if the user is a twitch staff member.
        /// </summary>
        public static bool IsDiscordOrTwitchStaff(IUser user) {
            if (user is SocketGuildUser socketUser) {

                // Discord staff
                if (IsDiscordStaff(user)) {
                    return true;
                }

                // Twitch staff
                if (socketUser.Roles.Any(x => TwitchStaffRoles.Contains(x.Id))) {
                    return true;
                }

                return false;
            } else {
                return false;
            }
        }

    }

}
