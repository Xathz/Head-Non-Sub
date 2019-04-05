using System;
using System.Collections.Generic;
using System.Linq;
using HeadNonSub.Database.Tables;
using HeadNonSub.Entities.Database.UserNote;
using Microsoft.EntityFrameworkCore;

namespace HeadNonSub.Database {

    public static partial class DatabaseManager {

        public static class UserNotes {

            /// <summary>
            /// Insert a user note.
            /// </summary>
            /// <param name="serverId">Server (guild) id.</param>
            /// <param name="userId">User id of who this note is for.</param>
            /// <param name="note">Note text.</param>
            public static void Insert(ulong serverId, ulong userId, Note note) {
                try {
                    using (DatabaseContext database = new DatabaseContext()) {
                        if (database.UserNotes.AsNoTracking().Any(x => x.ServerId == serverId & x.UserId == userId)) {
                            UserNote userNote = database.UserNotes.FirstOrDefault(x => x.ServerId == serverId & x.UserId == userId);
                            userNote.Notes.Add(note);

                            database.UserNotes.Update(userNote);

                        } else {
                            UserNote item = new UserNote {
                                ServerId = serverId,
                                UserId = userId,
                                Notes = new List<Note> { note }
                            };

                            database.UserNotes.Add(item);
                        }

                        database.SaveChanges();
                    }
                } catch (Exception ex) {
                    LoggingManager.Log.Error(ex);
                }
            }

            /// <summary>
            /// Get a users notes.
            /// </summary>
            /// <param name="serverId">Server (guild) id.</param>
            /// <param name="userId">User id of who to get notes about.</param>
            /// <returns>List of notes.</returns>
            public static List<Note> GetUser(ulong serverId, ulong userId) {
                try {
                    using (DatabaseContext database = new DatabaseContext()) {
                        if (database.UserNotes.AsNoTracking().Any(x => x.ServerId == serverId & x.UserId == userId)) {
                            return database.UserNotes.Where(x => x.ServerId == serverId & x.UserId == userId).SelectMany(x => x.Notes).OrderByDescending(x => x.DateTime).ToList();
                        } else {
                            return new List<Note>();
                        }
                    }
                } catch (Exception ex) {
                    LoggingManager.Log.Error(ex);
                    return new List<Note>();
                }
            }

            /// <summary>
            /// Delete a user note.
            /// </summary>
            /// <param name="serverId">Server (guild) id.</param>
            /// <param name="userId">User id of who this note id is for.</param>
            /// <param name="noteId">Note id to delete.</param>
            /// <returns>True if deleted; false if id does not exist.</returns>
            public static bool Delete(ulong serverId, ulong userId, string noteId) {
                try {
                    using (DatabaseContext database = new DatabaseContext()) {
                        if (database.UserNotes.AsNoTracking().Any(x => x.ServerId == serverId & x.UserId == userId)) {
                            UserNote userNote = database.UserNotes.FirstOrDefault(x => x.ServerId == serverId & x.UserId == userId);
                            userNote.Notes.RemoveAll(x => x.Id == noteId);

                            database.UserNotes.Update(userNote);
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

            /// <summary>
            /// Delete all notes about a user.
            /// </summary>
            /// <param name="serverId">Server (guild) id.</param>
            /// <param name="userId">User id of who to delete all notes about.</param>
            /// <returns>True if all were deleted; false if no notes.</returns>
            public static bool DeleteAll(ulong serverId, ulong userId) {
                try {
                    using (DatabaseContext database = new DatabaseContext()) {
                        if (database.UserNotes.AsNoTracking().Any(x => x.ServerId == serverId & x.UserId == userId)) {
                            UserNote userNote = database.UserNotes.FirstOrDefault(x => x.ServerId == serverId & x.UserId == userId);
                            database.UserNotes.Remove(userNote);

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
