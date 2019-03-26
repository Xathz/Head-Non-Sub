using System;
using System.Collections.Generic;

namespace HeadNonSub.Clients.Discord {

    public class RaidRecoveryTracker {

        private static Dictionary<ulong, Event> _ActiveChannels = new Dictionary<ulong, Event>();

        /// <summary>
        /// Track a new event.
        /// </summary>
        /// <param name="channel">Channel id.</param>
        public static void Track(ulong channel) {
            _ActiveChannels.Add(channel, new Event());
        }

        /// <summary>
        /// Check if an event exists.
        /// </summary>
        /// <param name="channel">Channel id.</param>
        public static bool Exists(ulong channel) {
            return _ActiveChannels.ContainsKey(channel);
        }

        /// <summary>
        /// Untrack a event.
        /// </summary>
        /// <param name="channel">Channel id.</param>
        public static void Untrack(ulong channel) {
            _ActiveChannels.Remove(channel);
        }

        /// <summary>
        /// Get the ban token for this event.
        /// </summary>
        /// <param name="channel">Channel id.</param>
        public static string BanToken(ulong channel) {
            if (_ActiveChannels.ContainsKey(channel)) {
                return _ActiveChannels[channel].BanToken;
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
            if (_ActiveChannels.ContainsKey(channel)) {
                if (_ActiveChannels[channel].BanToken == token) {
                    _ActiveChannels[channel].ValidBanToken = true;
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
            if (_ActiveChannels.ContainsKey(channel)) {
                _ActiveChannels[channel].UsersToBan.UnionWith(users);
            }
        }

        /// <summary>
        /// Get the list of users to ban.
        /// </summary>
        /// <param name="channel">Channel id.</param>
        public static HashSet<ulong> UsersToBan(ulong channel) {
            if (_ActiveChannels.ContainsKey(channel)) {
                return _ActiveChannels[channel].UsersToBan;
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
            if (_ActiveChannels.ContainsKey(channel)) {
                return _ActiveChannels[channel].UsersToBan.Remove(user);
            } else {
                return false;
            }
        }

        private class Event {

            public string BanToken { get; private set; } = Guid.NewGuid().ToString("N").Substring(0, 12);
            public bool ValidBanToken { get; set; } = false;
            public HashSet<ulong> UsersToBan { get; set; } = new HashSet<ulong>();

        }

    }

}
