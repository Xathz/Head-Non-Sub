﻿using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace HeadNonSub.Clients.Discord {

    /// <summary>
    /// Wubby's Fun House
    /// </summary>
    public static class WubbysFunHouse {

        public const ulong AdminsRoleId = 372244721625464845;

        public const ulong ModsRoleId = 336022934621519874;

        public const ulong ModLiteRoleId = 497550793923362827;

        public const ulong TwitchModRoleId = 468960777978642443;

        public const ulong SubredditModRoleId = 542135412106330122;

        public const ulong BotsRoleId = 328403426142715906;

        public const ulong TwitchSubscriberRoleId = 428052879371272192;

        public const ulong PatronRoleId = 328732005024137217;

        public const ulong NonSubRoleId = 508752510216044547;

        /// <summary>
        /// Admins, mods, and mod-lites.
        /// </summary>
        public static readonly ReadOnlyCollection<ulong> DiscordStaffRoles = new ReadOnlyCollection<ulong>(new List<ulong> { AdminsRoleId, ModsRoleId, ModLiteRoleId });

        /// <summary>
        /// Admins, mods, mod-lites, twitch mods, and subreddit mods.
        /// </summary>
        public static readonly ReadOnlyCollection<ulong> AllStaffRoles = new ReadOnlyCollection<ulong>(new List<ulong> { AdminsRoleId, ModsRoleId, ModLiteRoleId, TwitchModRoleId, SubredditModRoleId });

        public const ulong ActualFuckingSpamChannelId = 537727672747294738;

    }

}