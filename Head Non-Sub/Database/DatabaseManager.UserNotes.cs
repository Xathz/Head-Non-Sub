using System;
using System.Collections.Generic;
using System.Linq;
using HeadNonSub.Database.Tables;
using HeadNonSub.Entities.Database.UserNote;
using Microsoft.EntityFrameworkCore;

namespace HeadNonSub.Database {

    public static partial class DatabaseManager {

        public static class UserNotes {

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
