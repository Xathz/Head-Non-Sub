using System;
using System.Collections.Generic;
using System.Linq;
using HeadNonSub.Database.Tables;
using Microsoft.EntityFrameworkCore;

namespace HeadNonSub.Database {

    public static partial class DatabaseManager {

        public static class DynamicCommands {

            /// <summary>
            /// Insert a dynamic command.
            /// </summary>
            /// <param name="ownerId">Command owner user id.</param>
            /// <param name="command">Command name.</param>
            /// <param name="text">Command value: text, link, whatever.</param>
            /// <returns>Tuple (true if inserted; false if other, reason).</returns>
            public static (bool successful, string reason) Insert(ulong ownerId, string command, string text) {
                try {
                    using (DatabaseContext database = new DatabaseContext()) {
                        if (database.DynamicCommands.AsNoTracking().Any(x => x.OwnerId == ownerId)) {
                            return (false, "You have already claimed a command!");
                        } else if (database.DynamicCommands.AsNoTracking().Any(x => x.Command == command)) {
                            return (false, $"`-{command}` was already claimed.");
                        } else {
                            database.DynamicCommands.Add(new DynamicCommand { OwnerId = ownerId, DateTime = DateTime.UtcNow, Command = command, Text = text });
                            database.SaveChanges();

                            return (true, $"`-{command}` claimed successfully!");
                        }
                    }
                } catch (Exception ex) {
                    LoggingManager.Log.Error(ex);
                    return (false, $"There was an error claiming `-{command}`.");
                }
            }

            /// <summary>
            /// Get all dynamic commands.
            /// </summary>
            /// <returns>Dictionary (command, text).</returns>
            public static Dictionary<string, string> GetAll() {
                try {
                    using (DatabaseContext database = new DatabaseContext()) {
                        return database.DynamicCommands.Select(x => new { x.Command, x.Text }).ToDictionary(x => x.Command, x => x.Text);
                    }
                } catch (Exception ex) {
                    LoggingManager.Log.Error(ex);
                    return new Dictionary<string, string>();
                }
            }

            /// <summary>
            /// Who owns a command.
            /// </summary>
            /// <param name="command">Command name.</param>
            /// <returns>User id; null if not owned.</returns>
            public static ulong? Who(string command) {
                try {
                    using (DatabaseContext database = new DatabaseContext()) {
                        if (database.DynamicCommands.AsNoTracking().Any(x => x.Command == command)) {
                            return database.DynamicCommands.Where(x => x.Command == command).Select(x => x.OwnerId).FirstOrDefault();
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
