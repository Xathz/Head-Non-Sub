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

        public const string TwitchSubscribeUrl = "https://wub.by/subttv";

        public const string LinkTwitchToDiscordUrl = "https://wub.by/linkttv";

        #endregion

        #region Channel ids

        public const ulong MainChannelId = 403341336129830918;

        public const ulong ActualFuckingSpamChannelId = 537727672747294738;

        public const ulong ModLogsChannelId = 502940074036690954;

        public const ulong UserLogsChannelId = 490766846761500683;

        #endregion

        #region Channel category ids

        public const ulong ModMailCategoryId = 675632145749770297;

        #endregion

        #region User ids

        public const ulong PaymoneyWubbyUserId = 177657233025400832;

        #endregion

        #region Bot ids

        public const ulong WubbyMailBotUserId = 523299238369820692;

        #endregion

        #region Role ids

        public const ulong GingerBoyRoleId = 465872398772862976;

        public const ulong AdminsRoleId = 372244721625464845;

        public const ulong ModsRoleId = 336022934621519874;

        public const ulong ModLiteRoleId = 497550793923362827;

        public const ulong TwitchModRoleId = 468960777978642443;

        public const ulong SubredditModRoleId = 542135412106330122;

        public const ulong PatreonLilBroRoleId = 1488258271046533240;

        public const ulong PatreonRealOneRoleId = 1488259881919053895;

        public const ulong TwitchSubscriberTier1RoleId = 1455366512549298248;

        public const ulong TwitchSubscriberTier2RoleId = 1455366512549298249;

        public const ulong TwitchSubscriberTier3RoleId = 1455366512549298250;

        public const ulong TwitchSubscriberRoleId = 1455366512549298247;

        public const ulong MutedRoleId = 445807715655221259;

        /// <summary>
        /// Admins, mods, and mod-lites.
        /// </summary>
        public static readonly ReadOnlyCollection<ulong> DiscordStaffRoles = new ReadOnlyCollection<ulong>(new List<ulong> { GingerBoyRoleId, AdminsRoleId, ModsRoleId, ModLiteRoleId });

        /// <summary>
        /// Admins, mods, mod-lites, and twitch mods.
        /// </summary>
        public static readonly ReadOnlyCollection<ulong> TwitchStaffRoles = new ReadOnlyCollection<ulong>(new List<ulong> { GingerBoyRoleId, AdminsRoleId, ModsRoleId, ModLiteRoleId, TwitchModRoleId });

        #endregion

        /// <summary>
        /// Get if the user is a subscriber.
        /// </summary>
        public static bool IsSubscriber(IUser user) {
            if (user is SocketGuildUser socketUser) {

                if (socketUser.Roles.Any(x =>

                    // Twitch subscriber
                    x.Id == TwitchSubscriberRoleId ||
                    x.Id == TwitchSubscriberTier1RoleId ||
                    x.Id == TwitchSubscriberTier2RoleId ||
                    x.Id == TwitchSubscriberTier3RoleId ||

                    // Patreon subscriber
                    x.Id == PatreonLilBroRoleId ||
                    x.Id == PatreonRealOneRoleId)) {

                    return true;
                }

                return false;
            } else {
                return false;
            }
        }

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
