using System;
using System.Collections.Generic;
using System.Linq;
using HeadNonSub.Database.Tables;
using Microsoft.EntityFrameworkCore;

namespace HeadNonSub.Database {

    public static partial class DatabaseManager {

        public static class Cooldowns {

            /// <summary>
            /// Insert a cooldown.
            /// </summary>
            /// <param name="serverId">Server (guild) id.</param>
            /// <param name="userId">User id of who executed.</param>
            /// <param name="command">Name of the command.</param>
            public static bool Insert(ulong serverId, ulong userId, string command) {
                try {
                    using DatabaseContext database = new DatabaseContext();

                    if (database.Cooldowns.AsNoTracking().Any(x => x.ServerId == serverId & x.UserId == userId & x.Command == command)) {
                        return false;
                    } else {
                        database.Cooldowns.Add(new Cooldown() { DateTimeOffset = DateTimeOffset.UtcNow, ServerId = serverId, UserId = userId, Command = command });

                        database.SaveChanges();
                        return true;
                    }
                } catch (Exception ex) {
                    LoggingManager.Log.Error(ex);
                    return false;
                }
            }

            /// <summary>
            /// Check if a cooldown exists.
            /// </summary>
            /// <param name="serverId">Server (guild) id.</param>
            /// <param name="userId">User id of who executed.</param>
            /// <param name="command">Name of the command.</param>
            /// <param name="perUser">Is the command per-user (true) or server wide (false).</param>
            /// <returns>DateTimeOffset if found; null if not found.</returns>
            public static DateTimeOffset? Check(ulong serverId, ulong userId, string command, bool perUser) {
                try {
                    using DatabaseContext database = new DatabaseContext();

                    if (perUser) {
                        if (database.Cooldowns.AsNoTracking().Any(x => x.ServerId == serverId & x.UserId == userId & x.Command == command)) {
                            return database.Cooldowns.AsNoTracking().FirstOrDefault(x => x.ServerId == serverId & x.UserId == userId & x.Command == command).DateTimeOffset;
                        } else {
                            return null;
                        }
                    } else {
                        if (database.Cooldowns.AsNoTracking().Any(x => x.ServerId == serverId & x.Command == command)) {
                            return database.Cooldowns.AsNoTracking().FirstOrDefault(x => x.ServerId == serverId & x.Command == command).DateTimeOffset;
                        } else {
                            return null;
                        }
                    }
                } catch (Exception ex) {
                    LoggingManager.Log.Error(ex);
                    return null;
                }
            }

            /// <summary>
            /// Delete a cooldown.
            /// </summary>
            /// <param name="serverId">Server (guild) id.</param>
            /// <param name="userId">User id of who executed.</param>
            /// <param name="command">Name of the command.</param>
            /// <param name="perUser">Is the command per-user (true) or server wide (false).</param>
            /// <returns>True if deleted; false if id does not exist.</returns>
            public static bool Delete(ulong serverId, ulong userId, string command, bool perUser) {
                try {
                    using DatabaseContext database = new DatabaseContext();

                    if (perUser) {
                        if (database.Cooldowns.AsNoTracking().Any(x => x.ServerId == serverId & x.UserId == userId & x.Command == command)) {
                            List<Cooldown> cooldowns = database.Cooldowns.AsNoTracking().Where(x => x.ServerId == serverId & x.UserId == userId & x.Command == command).ToList();

                            database.Cooldowns.RemoveRange(cooldowns);

                            database.SaveChanges();
                            return true;
                        } else {
                            return false;
                        }
                    } else {
                        if (database.Cooldowns.AsNoTracking().Any(x => x.ServerId == serverId & x.Command == command)) {
                            List<Cooldown> cooldowns = database.Cooldowns.AsNoTracking().Where(x => x.ServerId == serverId & x.Command == command).ToList();

                            database.Cooldowns.RemoveRange(cooldowns);

                            database.SaveChanges();
                            return true;
                        } else {
                            return false;
                        }
                    }
                } catch (Exception ex) {
                    LoggingManager.Log.Error(ex);
                    return false;
                }
            }

        }

    }

}
