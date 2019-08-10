using System;
using System.Collections.Concurrent;

namespace HeadNonSub.Clients.Discord {

    public static class EmoteModeTracker {

        private static readonly ConcurrentDictionary<ulong, Mode> _ActiveModes = new ConcurrentDictionary<ulong, Mode>();

        private static readonly ConcurrentDictionary<ulong, DateTimeOffset> _NotificationTimers = new ConcurrentDictionary<ulong, DateTimeOffset>();

        /// <summary>
        /// Set a channel's mode.
        /// </summary>
        /// <param name="channel">Channel id.</param>
        /// <param name="mode">Emote mode.</param>
        public static void SetMode(ulong channel, Mode mode) {
            _ActiveModes.AddOrUpdate(channel, mode, (existingChannel, existingMode) => mode);
            _NotificationTimers.AddOrUpdate(channel, DateTimeOffset.UtcNow, (existingChannel, existingMode) => DateTimeOffset.UtcNow);
        }

        /// <summary>
        /// Get a channel's mode.
        /// </summary>
        /// <param name="channel">Channel Id.</param>
        public static Mode GetMode(ulong channel) {
            if (_ActiveModes.TryGetValue(channel, out Mode mode)) {
                return mode;
            } else {
                return Mode.Off;
            }
        }

        /// <summary>
        /// Should a reminder notification be sent.
        /// </summary>
        /// <param name="channel">Channel id.</param>
        public static bool ShouldSendNotification(ulong channel) {
            if (_NotificationTimers.TryGetValue(channel, out DateTimeOffset offset)) {
                if (offset.AddSeconds(30) < DateTimeOffset.UtcNow) {
                    _NotificationTimers.AddOrUpdate(channel, DateTimeOffset.UtcNow, (existingChannel, existingMode) => DateTimeOffset.UtcNow);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Remove mode from a channel.
        /// </summary>
        /// <param name="channel">Channel id.</param>
        public static void RemoveMode(ulong channel) {
            _ActiveModes.TryRemove(channel, out Mode _);
            _NotificationTimers.TryRemove(channel, out DateTimeOffset _);
        }

        /// <summary>
        /// Channel emote/emoji mode.
        /// </summary>
        public enum Mode {
            Off,
            TextOnly,
            EmoteOnly
        }

    }

}
