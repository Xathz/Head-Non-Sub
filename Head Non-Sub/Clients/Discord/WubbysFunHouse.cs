using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace HeadNonSub.Clients.Discord {

    /// <summary>
    /// Wubby's Fun House
    /// </summary>
    public static class WubbysFunHouse {

        public const ulong ServerId = 328300333010911242;

        public const ulong GingerBoyRoleId = 465872398772862976;

        public const ulong AdminsRoleId = 372244721625464845;

        public const ulong ModsRoleId = 336022934621519874;

        public const ulong ModLiteRoleId = 497550793923362827;

        public const ulong TwitchModRoleId = 468960777978642443;

        public const ulong SubredditModRoleId = 542135412106330122;

        public const ulong BotsRoleId = 328403426142715906;

        public const ulong TwitchSubscriberRoleId = 428052879371272192;

        public const ulong PatronRoleId = 328732005024137217;

        public const ulong NonSubRoleId = 508752510216044547;

        public const ulong MutedRoleId = 445807715655221259;

        public const ulong Tier3RoleId = 493641643765792768;

        public const ulong RuleReaderRoleId = 580577184809353226;

        /// <summary>MEE6 rank 10</summary>
        /// <remarks>
        /// https://wub.by/discordranks
        /// https://mee6.xyz/leaderboard/328300333010911242
        /// </remarks>
        public const ulong NakedCowboyRoleId = 502699098646904833;

        /// <summary>
        /// Admins, mods, and mod-lites.
        /// </summary>
        public static readonly ReadOnlyCollection<ulong> DiscordStaffRoles = new ReadOnlyCollection<ulong>(new List<ulong> { AdminsRoleId, ModsRoleId, ModLiteRoleId });

        /// <summary>
        /// Admins, mods, mod-lites, and twitch mods.
        /// </summary>
        public static readonly ReadOnlyCollection<ulong> TwitchStaffRoles = new ReadOnlyCollection<ulong>(new List<ulong> { AdminsRoleId, ModsRoleId, ModLiteRoleId, TwitchModRoleId });

        /// <summary>
        /// Admins, mods, mod-lites, twitch mods, and subreddit mods.
        /// </summary>
        public static readonly ReadOnlyCollection<ulong> AllStaffRoles = new ReadOnlyCollection<ulong>(new List<ulong> { AdminsRoleId, ModsRoleId, ModLiteRoleId, TwitchModRoleId, SubredditModRoleId });

        public const ulong MainChannelId = 403341336129830918;

        public const ulong LinksChannelId = 553407177654665216;

        public const ulong MarketResearchChannelId = 526223101101604873;

        public const ulong ActualFuckingSpamChannelId = 537727672747294738;

        public const ulong ModLogsChannelId = 502940074036690954;

        public const ulong UserLogsChannelId = 490766846761500683;

    }

}
