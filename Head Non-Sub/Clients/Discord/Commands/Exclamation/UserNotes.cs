using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using HeadNonSub.Clients.Discord.Attributes;
using HeadNonSub.Entities.Database.UserNote;

namespace HeadNonSub.Clients.Discord.Commands.Exclamation {

    [BlacklistEnforced]
    [OwnerAdminWhitelist]
    [RequireContext(ContextType.Guild)]
    public class UserNotes : BetterModuleBase {

        [Command("notes")]
        public Task Notes(SocketUser user = null) {
            if (user == null) {
                return BetterReplyAsync("You must mention a user to get their notes.");
            }

            List<Note> notes = Database.DatabaseManager.GetNotes(Context.Guild.Id, user.Id);

            EmbedBuilder builder = new EmbedBuilder() {
                Color = new Color(Constants.GeneralColor.R, Constants.GeneralColor.G, Constants.GeneralColor.B),
                Title = $"Notes about {BetterUserFormat(user)}"
            };

            if (notes.Count > 0) {
                foreach (Note note in notes) {
                    builder.AddField($"{note.DateTime.ToString(Constants.DateTimeFormatShort).ToLower()} utc ● {BetterUserFormat(UserFromUserId(note.UserId))} ● {note.Id}", note.Text);
                }
            } else {
                builder.Title = $"There are no notes about {BetterUserFormat(user)}";
            }

            return BetterReplyAsync(builder.Build(), $"{user.ToString()} ({user.Id})");
        }

        [Command("addnote")]
        public Task AddNote(SocketUser user = null, [Remainder]string note = "") {
            if (user == null) {
                return BetterReplyAsync("You must mention a user to add a note.");
            }

            if (string.IsNullOrWhiteSpace(note)) {
                return BetterReplyAsync($"You did not enter a note to add to {user.ToString()}");
            }

            Note item = new Note {
                DateTime = DateTime.UtcNow,
                UserId = Context.User.Id,
                Text = note
            };

            Database.DatabaseManager.InsertNote(Context.Guild.Id, user.Id, item);

            return BetterReplyAsync($"Note added about {BetterUserFormat(user)}.", $"{user.ToString()} ({user.Id}); {note}");
        }

        [Command("deletenote")]
        public Task DeleteNote(SocketUser user = null, [Remainder]string noteId = "") {
            if (user == null) {
                return BetterReplyAsync("You must mention a user to delete a note about them.");
            }

            if (string.IsNullOrWhiteSpace(noteId)) {
                return BetterReplyAsync($"You did not enter a note id to delete.", $"{user.ToString()} ({user.Id}); {noteId}");
            } else {
                if (Database.DatabaseManager.DeleteNote(Context.Guild.Id, user.Id, noteId)) {
                    return BetterReplyAsync($"Note about {BetterUserFormat(user)} was deleted.", $"{user.ToString()} ({user.Id}); {noteId}");
                } else {
                    return BetterReplyAsync($"Invalid note id.", $"{user.ToString()} ({user.Id}); {noteId}");
                }
            }
        }

        [Command("deleteallnotes")]
        public Task DeleteAllNotes(SocketUser user = null) {
            if (user == null) {
                return BetterReplyAsync("You must mention a user to delete all notes about them.");
            }

            if (Database.DatabaseManager.DeleteAllNotes(Context.Guild.Id, user.Id)) {
                return BetterReplyAsync($"All notes about {BetterUserFormat(user)} were deleted.", $"{user.ToString()} ({user.Id})");
            } else {
                return BetterReplyAsync($"There are no notes about {BetterUserFormat(user)}.", $"{user.ToString()} ({user.Id})");
            }
        }

    }

}
