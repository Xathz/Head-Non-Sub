using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace HeadNonSub.Clients.Discord {

    public class RaidRecoveryTracker {

        private static ConcurrentDictionary<ulong, ActiveRaid> _ActiveRaids = new ConcurrentDictionary<ulong, ActiveRaid>();

        /// <summary>
        /// Track a new event.
        /// </summary>
        /// <param name="channel">Channel id.</param>
        /// <param name="startedBy">User id of who started the event.</param>
        /// <param name="slowModeInterval">Slow mode interval of the channel.</param>
        public static bool Track(ulong channel, ulong startedBy, int slowModeInterval) => _ActiveRaids.TryAdd(channel, new ActiveRaid(startedBy, slowModeInterval));

        /// <summary>
        /// Check if an event exists.
        /// </summary>
        /// <param name="channel">Channel id.</param>
        public static bool Exists(ulong channel) => _ActiveRaids.ContainsKey(channel);

        /// <summary>
        /// Untrack a event.
        /// </summary>
        /// <param name="channel">Channel id.</param>
        public static bool Untrack(ulong channel) => _ActiveRaids.TryRemove(channel, out ActiveRaid _);

        /// <summary>
        /// Get who started an event.
        /// </summary>
        /// <param name="channel">Channel id.</param>
        public static ulong? StartedBy(ulong channel) {
            if (_ActiveRaids.TryGetValue(channel, out ActiveRaid activeRaid)) {
                return activeRaid.StartedBy;
            } else {
                return null;
            }
        }

        /// <summary>
        /// Get the ban token for this event.
        /// </summary>
        /// <param name="channel">Channel id.</param>
        public static string BanToken(ulong channel) {
            if (_ActiveRaids.TryGetValue(channel, out ActiveRaid activeRaid)) {
                return activeRaid.BanToken;
            } else {
                return string.Empty;
            }
        }

        /// <summary>
        /// Check if a ban token is valid.
        /// </summary>
        /// <param name="channel">Channel id.</param>
        /// <param name="token">Ban token to check.</param>
        public static bool ValidateBanToken(ulong channel, string token) {
            if (_ActiveRaids.TryGetValue(channel, out ActiveRaid activeRaid)) {
                if (activeRaid.BanToken == token) {
                    _ActiveRaids[channel].ValidBanToken = true;
                    return true;
                } else {
                    return false;
                }
            } else {
                return false;
            }
        }

        /// <summary>
        /// Add a list of users to ban.
        /// </summary>
        /// <param name="channel">Channel id.</param>
        /// <param name="users">List of users to ban.</param>
        public static void AddUsersToBan(ulong channel, HashSet<ulong> users) {
            if (_ActiveRaids.ContainsKey(channel)) {
                _ActiveRaids[channel].UsersToBan.UnionWith(users);
            }
        }

        /// <summary>
        /// Get the list of users to ban.
        /// </summary>
        /// <param name="channel">Channel id.</param>
        public static HashSet<ulong> UsersToBan(ulong channel) {
            if (_ActiveRaids.TryGetValue(channel, out ActiveRaid activeRaid)) {
                return activeRaid.UsersToBan;
            } else {
                return new HashSet<ulong>();
            }
        }

        /// <summary>
        /// Remove a user from the users to ban list.
        /// </summary>
        /// <param name="channel">Channel id.</param>
        /// <param name="user">User id to remove from users to ban.</param>
        public static bool SkipUserToBan(ulong channel, ulong user) {
            if (_ActiveRaids.ContainsKey(channel)) {
                return _ActiveRaids[channel].UsersToBan.Remove(user);
            } else {
                return false;
            }
        }

        /// <summary>
        /// Get the channel's previous slow mode interval.
        /// </summary>
        /// <param name="channel">Channel id.</param>
        public static int PreviousSlowModeInterval(ulong channel) {
            if (_ActiveRaids.TryGetValue(channel, out ActiveRaid activeRaid)) {
                return activeRaid.PreviousSlowModeInterval;
            } else {
                return 0;
            }
        }

        private class ActiveRaid {

            public ActiveRaid(ulong startedBy, int previousSlowModeInterval) {
                StartedBy = startedBy;
                PreviousSlowModeInterval = previousSlowModeInterval;
            }

            public ulong StartedBy { get; private set; }
            public int PreviousSlowModeInterval { get; private set; }
            public string BanToken { get; private set; } = Guid.NewGuid().ToString("N").Substring(0, 12);
            public bool ValidBanToken { get; set; } = false;
            public HashSet<ulong> UsersToBan { get; set; } = new HashSet<ulong>();

        }

    }

}
