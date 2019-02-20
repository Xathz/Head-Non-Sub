using System;
using System.Linq;
using System.Collections.Generic;

namespace HeadNonSub.Clients.Discord {

    public class CooldownTracker {

        private static List<(DateTimeOffset dateTimeOffset, ulong server, ulong userId, string command)> _AttemptedCommands = new List<(DateTimeOffset dateTimeOffset, ulong server, ulong userId, string command)>();

        /// <summary>
        /// Track a command.
        /// </summary>
        /// <param name="server">Server (guild) id.</param>
        /// <param name="userId">Invoking user id.</param>
        /// <param name="command">Name of the command attempted.</param>
        public static void Track(ulong server, ulong userId, string command) {
            if (!_AttemptedCommands.Any(x => x.server == server & x.userId == userId & x.command == command)) {
                _AttemptedCommands.Add((DateTimeOffset.Now, server, userId, command));
            }

            // Remove oldest
            if (_AttemptedCommands.Count > 1000) {
                _AttemptedCommands.RemoveAt(0);
            }
        }

        /// <summary>
        /// Check if user is allowed to run command based on a cooldown.
        /// </summary>
        /// <param name="server">Server (guild) id.</param>
        /// <param name="userId">Invoking user id.</param>
        /// <param name="command">Name of the command attempted.</param>
        /// <param name="cooldown">How long is this cooldown (in seconds).</param>
        /// <param name="perUser">Is the command per-user? <see langword="true"/> = per-user; <see langword="false"/> = per command.</param>
        public static (bool isAllowed, int secondsRemaining) IsAllowed(ulong server, ulong userId, string command, int cooldown, bool perUser) {
            DateTimeOffset dateTimeOffset;

            if (perUser) {
                dateTimeOffset = _AttemptedCommands.Where(x => x.server == server & x.userId == userId & x.command == command).Select(x => x.dateTimeOffset).FirstOrDefault();
            } else {
                dateTimeOffset = _AttemptedCommands.Where(x => x.server == server & x.command == command).Select(x => x.dateTimeOffset).FirstOrDefault();
            }

            if (dateTimeOffset != null) {
                if (dateTimeOffset.AddSeconds(cooldown) >= DateTimeOffset.Now) {
                    return (false, (int)(dateTimeOffset.AddSeconds(cooldown) - DateTimeOffset.Now).TotalSeconds);
                } else {
                    _AttemptedCommands.RemoveAll(x => x.server == server & x.userId == userId & x.command == command);

                    return (true, 0);
                }
            } else {
                return (true, 0);
            }
        }

    }

}
