using System;
using Discord.WebSocket;
using HeadNonSub.Database;
using Humanizer;

namespace HeadNonSub.Clients.Discord {

    public static class Cooldowns {

        /// <summary>
        /// Check and start a cooldown. Discord staff are not affected.
        /// </summary>
        /// <param name="user">User running the command.</param>
        /// <param name="key">Unique name for the cooldown, usually the command name.</param>
        /// <param name="seconds">Length of the cooldown in seconds.</param>
        /// <param name="perUser">Is the cooldown per-user (true) or server wide (false).</param>
        /// <returns>Error message if still on cooldown; <see langword="null"/> if the command can run.</returns>
        public static string Check(SocketGuildUser user, string key, int seconds, bool perUser) {
            if (WubbysFunHouse.IsDiscordStaff(user)) {
                return null;
            }

            DateTimeOffset? started = DatabaseManager.Cooldowns.TryStart(user.Guild.Id, user.Id, key, perUser, TimeSpan.FromSeconds(seconds));

            if (started.HasValue) {
                string remaining = (started.Value.AddSeconds(seconds) - DateTimeOffset.UtcNow).TotalSeconds.Seconds().Humanize();

                if (perUser) {
                    return $"You need to wait {remaining} before you can use `{key}` again. Cooldown is per-user.";
                } else {
                    return $"You need to wait {remaining} before you can use `{key}` again. Cooldown is server wide.";
                }
            } else {
                LoggingManager.Log.Debug($"Command \"{key}\" ({seconds}sec) {(perUser ? "per-user" : "server wide")} cooldown started {(perUser ? $"for {user} " : "")}in {user.Guild.Name}");
                return null;
            }
        }

    }

}
