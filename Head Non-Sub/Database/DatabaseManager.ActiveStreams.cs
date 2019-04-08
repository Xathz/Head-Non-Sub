using System;
using System.Linq;
using HeadNonSub.Database.Tables;
using Microsoft.EntityFrameworkCore;

namespace HeadNonSub.Database {

    public static partial class DatabaseManager {

        public static class ActiveStreams {

            /// <summary>
            /// Insert a live Twitch stream.
            /// </summary>
            /// <param name="username">Username of the stream.</param>
            /// <returns>True if inserted; false if exists.</returns>
            public static bool Insert(string username) {
                try {
                    using (DatabaseContext database = new DatabaseContext()) {
                        if (database.ActiveStreams.AsNoTracking().Any(x => x.Username == username)) {
                            return false;
                        } else {
                            database.ActiveStreams.Add(new ActiveStream() { Username = username, StartedAt = DateTime.UtcNow });

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
            /// Delete a live Twitch stream.
            /// </summary>
            /// <param name="username">Username of the stream.</param>
            /// <returns>DateTime of when stream went live, null if not deleted or error.</returns>
            public static DateTime? Delete(string username) {
                try {
                    using (DatabaseContext database = new DatabaseContext()) {
                        if (database.ActiveStreams.AsNoTracking().Any(x => x.Username == username)) {
                            ActiveStream stream = database.ActiveStreams.Where(x => x.Username == username).FirstOrDefault();
                            database.ActiveStreams.Remove(stream);

                            database.SaveChanges();

                            return stream.StartedAt;
                        } else {
                            return null;
                        }
                    }
                } catch (Exception ex) {
                    LoggingManager.Log.Error(ex);
                    return null;
                }
            }

        }

    }

}
