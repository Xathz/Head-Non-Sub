using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using HeadNonSub.Database.Tables;
using Microsoft.EntityFrameworkCore;

namespace HeadNonSub.Database {

    public static partial class DatabaseManager {

        public static class Stalking {

            /// <summary>
            /// Insert a new stalker.
            /// </summary>
            /// <param name="serverId">Server (guild) id.</param>
            /// <param name="userId">User id of the stalker.</param>
            /// <param name="stalkingUserId">User id of who is being stalked.</param>
            /// <returns>True if inserted; false if exists.</returns>
            public static bool Insert(ulong serverId, ulong userId, ulong stalkingUserId) {
                try {
                    using (DatabaseContext database = new DatabaseContext()) {
                        if (database.Stalkers.AsNoTracking().Any(x => x.ServerId == serverId & x.UserId == userId & x.StalkingUserId == stalkingUserId)) {
                            return false;
                        } else {
                            Stalker item = new Stalker {
                                ServerId = serverId,
                                UserId = userId,
                                StalkingUserId = stalkingUserId,
                                DateTime = DateTime.UtcNow
                            };

                            database.Stalkers.Add(item);
                            database.SaveChanges();

                            return true;
                        }
                    }
                } catch (Exception ex) {
                    LoggingManager.Log.Error(ex);
                    return false;
                }
            }

            /// <summary>
            /// Get all stalkers and who they are stalking.
            /// </summary>
            /// <param name="serverId">Server (guild) id.</param>
            public static ConcurrentBag<(ulong serverId, ulong userId, ulong stalkingUserId)> GetAll() {
                try {
                    using (DatabaseContext database = new DatabaseContext()) {
                        List<(ulong, ulong, ulong)> list = database.Stalkers.Select(x => Tuple.Create(x.ServerId, x.UserId, x.StalkingUserId)).Select(x => x.ToValueTuple()).ToList();
                        return new ConcurrentBag<(ulong serverId, ulong userId, ulong stalkingUserId)>(list);
                    }
                } catch (Exception ex) {
                    LoggingManager.Log.Error(ex);
                    return new ConcurrentBag<(ulong serverId, ulong userId, ulong stalkingUserId)>();
                }
            }

            /// <summary>
            /// Delete a stalker.
            /// </summary>
            /// <param name="serverId">Server (guild) id.</param>
            /// <param name="userId">User id of the stalker.</param>
            /// <param name="stalkingUserId">User id of who is being stalked.</param>
            /// <returns>True if deleted; false if exists.</returns>
            public static bool Delete(ulong serverId, ulong userId, ulong stalkingUserId) {
                try {
                    using (DatabaseContext database = new DatabaseContext()) {
                        if (database.Stalkers.AsNoTracking().Any(x => x.ServerId == serverId & x.UserId == userId & x.StalkingUserId == stalkingUserId)) {
                            Stalker stalker = database.Stalkers.FirstOrDefault(x => x.ServerId == serverId & x.UserId == userId & x.StalkingUserId == stalkingUserId);

                            database.Stalkers.Remove(stalker);
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
