using System;
using System.Collections.Generic;
using System.Linq;
using HeadNonSub.Database.Tables;

namespace HeadNonSub.Database {

    public static partial class DatabaseManager {

        public static class Cooldowns {

            // Only this process writes cooldowns, so a lock makes checking and starting one step
            private static readonly object _Lock = new object();

            /// <summary>
            /// Start a cooldown unless one is still running.
            /// </summary>
            /// <param name="serverId">Server (guild) id.</param>
            /// <param name="userId">User id of who executed.</param>
            /// <param name="command">Unique name for the cooldown, usually the command name.</param>
            /// <param name="perUser">Is the cooldown per-user (true) or server wide (false).</param>
            /// <param name="length">Length of the cooldown.</param>
            /// <returns>When the running cooldown started; <see langword="null"/> if a new one was started, or the database failed.</returns>
            public static DateTimeOffset? TryStart(ulong serverId, ulong userId, string command, bool perUser, TimeSpan length) {
                lock (_Lock) {
                    try {
                        using DatabaseContext database = new DatabaseContext();

                        IQueryable<Cooldown> query = database.Cooldowns.Where(x => x.ServerId == serverId & x.Command == command);
                        if (perUser) {
                            query = query.Where(x => x.UserId == userId);
                        }

                        List<Cooldown> cooldowns = query.ToList();
                        DateTimeOffset now = DateTimeOffset.UtcNow;

                        Cooldown running = cooldowns.Where(x => x.DateTimeOffset.Add(length) >= now).OrderByDescending(x => x.DateTimeOffset).FirstOrDefault();
                        if (running != null) {
                            return running.DateTimeOffset;
                        }

                        // Reuse this user's row, a server wide cooldown can also have expired rows from other users
                        Cooldown own = cooldowns.FirstOrDefault(x => x.UserId == userId);
                        database.Cooldowns.RemoveRange(cooldowns.Where(x => x != own));

                        if (own is null) {
                            database.Cooldowns.Add(new Cooldown() { DateTimeOffset = now, ServerId = serverId, UserId = userId, Command = command });
                        } else {
                            own.DateTimeOffset = now;
                        }

                        database.SaveChanges();
                        return null;
                    } catch (Exception ex) {
                        // Let the command run rather than block it when the database is down
                        LoggingManager.Log.Error(ex);
                        return null;
                    }
                }
            }

        }

    }

}
