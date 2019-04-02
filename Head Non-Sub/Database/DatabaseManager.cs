using System;
using System.Collections.Generic;
using System.Linq;
using HeadNonSub.Database.Tables;
using HeadNonSub.Entities.Database.UserNote;
using Microsoft.EntityFrameworkCore;

namespace HeadNonSub.Database {

    public static class DatabaseManager {

        private static DatabaseContext _Database;

        public static void Load() {
            if (_Database is null) {
                _Database = new DatabaseContext();

                LoggingManager.Log.Info("Connected");
            } else {
                LoggingManager.Log.Info("Loading has already completed");
            }
        }

        public static List<Note> GetNotes(ulong serverId, ulong userId) {
            try {
                if (_Database.UserNotes.AsNoTracking().Any(x => x.ServerId == serverId & x.UserId == userId)) {
                    return _Database.UserNotes.Where(x => x.ServerId == serverId & x.UserId == userId).SelectMany(x => x.Notes).OrderByDescending(x => x.DateTime).ToList();
                } else {
                    return new List<Note>();
                }
            } catch (Exception ex) {
                LoggingManager.Log.Error(ex);
                return new List<Note>();
            }
        }

        public static void InsertNote(ulong serverId, ulong userId, Note note) {
            try {
                if (_Database.UserNotes.AsNoTracking().Any(x => x.ServerId == serverId & x.UserId == userId)) {
                    UserNote userNote = _Database.UserNotes.FirstOrDefault(x => x.ServerId == serverId & x.UserId == userId);
                    userNote.Notes.Add(note);

                    _Database.UserNotes.Update(userNote);

                } else {
                    UserNote item = new UserNote {
                        ServerId = serverId,
                        UserId = userId,
                        Notes = new List<Note> { note }
                    };

                    _Database.UserNotes.Add(item);
                }

                _Database.SaveChanges();

            } catch (Exception ex) {
                LoggingManager.Log.Error(ex);
            }
        }

        public static bool DeleteNote(ulong serverId, ulong userId, string noteId) {
            try {
                if (_Database.UserNotes.AsNoTracking().Any(x => x.ServerId == serverId & x.UserId == userId)) {
                    UserNote userNote = _Database.UserNotes.FirstOrDefault(x => x.ServerId == serverId & x.UserId == userId);
                    userNote.Notes.RemoveAll(x => x.Id == noteId);

                    _Database.UserNotes.Update(userNote);
                    _Database.SaveChanges();

                    return true;
                } else {
                    return false;
                }
            } catch (Exception ex) {
                LoggingManager.Log.Error(ex);
                return false;
            }
        }

        public static bool DeleteAllNotes(ulong serverId, ulong userId) {
            try {
                if (_Database.UserNotes.AsNoTracking().Any(x => x.ServerId == serverId & x.UserId == userId)) {
                    UserNote userNote = _Database.UserNotes.FirstOrDefault(x => x.ServerId == serverId & x.UserId == userId);
                    _Database.UserNotes.Remove(userNote);

                    _Database.SaveChanges();

                    return true;
                } else {
                    return false;
                }
            } catch (Exception ex) {
                LoggingManager.Log.Error(ex);
                return false;
            }
        }

        public static Dictionary<string, string> GetDynamicCommands() {
            try {
                return _Database.DynamicCommands.Select(x => new { x.Command, x.Text }).ToDictionary(x => x.Command, x => x.Text);
            } catch (Exception ex) {
                LoggingManager.Log.Error(ex);
                return new Dictionary<string, string>();
            }
        }

    }

}
