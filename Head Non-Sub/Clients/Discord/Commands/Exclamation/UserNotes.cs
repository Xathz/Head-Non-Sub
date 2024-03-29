﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using HeadNonSub.Clients.Discord.Attributes;
using HeadNonSub.Database;
using HeadNonSub.Entities.Database.UserNote;

namespace HeadNonSub.Clients.Discord.Commands.Exclamation {

    [BlacklistEnforced]
    [DiscordStaffOnly]
    [RequireContext(ContextType.Guild)]
    public class UserNotes : BetterModuleBase {

        [Command("notes")]
        public async Task Notes(SocketUser user = null) {
            if (user == null) {
                await BetterReplyAsync("You must mention a user to get their notes.");
                return;
            }

            List<Note> notes = DatabaseManager.UserNotes.GetUser(Context.Guild.Id, user.Id);

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

            await BetterReplyAsync(builder.Build(), parameters: $"{user} ({user.Id})");
        }

        [Command("addnote")]
        public async Task AddNote(SocketUser user = null, [Remainder] string note = "") {
            if (user == null) {
                await BetterReplyAsync("You must mention a user to add a note.", parameters: $"user null; {note}");
                return;
            }

            if (string.IsNullOrWhiteSpace(note)) {
                await BetterReplyAsync($"You did not enter a note to add to {user}", parameters: $"{user} ({user.Id}); note null");
                return;
            }

            Note item = new Note {
                DateTime = DateTime.UtcNow,
                UserId = Context.User.Id,
                Text = note
            };

            DatabaseManager.UserNotes.Insert(Context.Guild.Id, user.Id, item);

            await BetterReplyAsync($"Note added about {BetterUserFormat(user)}.", $"{user} ({user.Id}); {note}");
        }

        [Command("deletenote")]
        public async Task DeleteNote(SocketUser user = null, [Remainder] string noteId = "") {
            if (user == null) {
                await BetterReplyAsync("You must mention a user to delete a note about them.", parameters: $"user null; {noteId}");
                return;
            }

            if (string.IsNullOrWhiteSpace(noteId)) {
                await BetterReplyAsync($"You did not enter a note id to delete.", $"{user} ({user.Id}); {noteId}");
            } else {
                if (DatabaseManager.UserNotes.Delete(Context.Guild.Id, user.Id, noteId)) {
                    await BetterReplyAsync($"Note about {BetterUserFormat(user)} was deleted.", $"{user} ({user.Id}); {noteId}");
                } else {
                    await BetterReplyAsync($"Invalid note id.", $"{user} ({user.Id}); {noteId}");
                }
            }
        }

        [Command("deleteallnotes")]
        public async Task DeleteAllNotes(SocketUser user = null) {
            if (user == null) {
                await BetterReplyAsync("You must mention a user to delete all notes about them.", parameters: $"user null");
                return;
            }

            if (DatabaseManager.UserNotes.DeleteAll(Context.Guild.Id, user.Id)) {
                await BetterReplyAsync($"All notes about {BetterUserFormat(user)} were deleted.", $"{user} ({user.Id})");
            } else {
                await BetterReplyAsync($"There are no notes about {BetterUserFormat(user)}.", $"{user} ({user.Id})");
            }
        }

    }

}
