using System;
using System.Collections.Generic;
using System.Linq;
using HeadNonSub.Database.Tables;
using HeadNonSub.Entities.Database.UserNote;
using Microsoft.EntityFrameworkCore;

namespace HeadNonSub.Database {

    public static class DatabaseManager {

        public static void Load() {
            using (DatabaseContext database = new DatabaseContext()) {
                if (database.Database.CanConnect()) {
                    if (database.Database.EnsureCreated()) {
                        LoggingManager.Log.Info("Database created");
                    } else {
                        LoggingManager.Log.Info("Database already exists, connected");
                    }
                } else {
                    LoggingManager.Log.Error("Can connect check failed");
                }
            }
        }

        public static List<Note> GetNotes(ulong serverId, ulong userId) {
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

        public static void InsertNote(ulong serverId, ulong userId, Note note) {
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

        public static bool DeleteNote(ulong serverId, ulong userId, string noteId) {
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

        public static bool DeleteAllNotes(ulong serverId, ulong userId) {
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

        public static Dictionary<string, string> GetDynamicCommands() {
            try {
                using (DatabaseContext database = new DatabaseContext()) {
                    return database.DynamicCommands.Select(x => new { x.Command, x.Text }).ToDictionary(x => x.Command, x => x.Text);
                }
            } catch (Exception ex) {
                LoggingManager.Log.Error(ex);
                return new Dictionary<string, string>();
            }
        }

        public static (bool successful, string reason) InsertDynamicCommand(ulong ownerId, string command, string text) {
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

        public static ulong? WhoDynamicCommand(string command) {
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
