using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Discord.Rest;
using Discord.Webhook;
using Discord.WebSocket;
using HeadNonSub.Clients.Discord.Attributes;
using HeadNonSub.Extensions;
using HeadNonSub.Settings;
using XathzEmotesEntities = HeadNonSub.Entities.Xathz.Emotes;

namespace HeadNonSub.Clients.Discord.Commands {

    [Group("xathz", "Xathz only tools")]
    [DefaultMemberPermissions(GuildPermission.ManageMessages)]
    [XathzOnly]
    [CommandContextType(InteractionContextType.Guild)]
    public class Xathz : BetterModuleBase {

        public enum Reaction {
            [ChoiceDisplay("allthis")]
            AllThis,
            [ChoiceDisplay("youareallretarded")]
            AllRetarded
        }

        [SlashCommand("puppet", "Make a user say something")]
        [DisallowedChannels(WubbysFunHouse.MainChannelId)]
        public async Task Puppet([Summary("user", "Who should talk")] SocketGuildUser user, [Summary("text", "What they should say")] string text) {
            if (!(Context.Channel is SocketTextChannel channel)) {
                await EphemeralReplyAsync("Puppets only work in text channels.");
                return;
            }

            await DeferAsync(true);

            Task<MemoryStream> download = Http.GetStreamAsync(user.GetDisplayAvatarUrl(ImageFormat.Png, 256));
            using MemoryStream avatar = await download;

            if (!download.IsCompletedSuccessfully) {
                await EphemeralReplyAsync("Failed to download their avatar.");
                return;
            }

            RestWebhook webhook = await channel.CreateWebhookAsync(BetterUserFormat(user, true), avatar);

            try {
                using (DiscordWebhookClient webhookClient = new DiscordWebhookClient(webhook)) {
                    await webhookClient.SendMessageAsync(text, allowedMentions: DiscordClient.UserMentionsOnly);
                }

                await LogUserMessageAsync("Head-Non Sub Puppet", $"User {Context.User} ({Context.User.Id}) made {user} ({user.Id}) say: {text}");
                TrackStatistics(parameters: $"{user} ({user.Id}); {text}");
                await EphemeralReplyAsync("Done.");
            } catch (Exception ex) {
                LoggingManager.Log.Error(ex);
                await EphemeralReplyAsync("Failed to send the message.");
            } finally {
                await webhook.DeleteAsync();
            }
        }

        [SlashCommand("searchemotes", "Get a random emote from emotes.xathz.net")]
        public async Task SearchEmotes([Summary("search", "What to search for")] string search) {
            await DeferAsync();

            search = search.Replace(" ", "").Trim().ToLowerInvariant();

            XathzEmotesEntities.XathzEmotes emotes = null;

            if (Cache.Get($"xathz_emotes_search_{search}") is XathzEmotesEntities.XathzEmotes cachedEmotes) {
                emotes = cachedEmotes;
            } else {
                Task<string> download = Http.SendRequestAsync($"https://emotes.xathz.net/api/?key={SettingsManager.Configuration.XathzEmotesKey}&search={search}");
                string data = await download;

                if (download.IsCompletedSuccessfully) {
                    XathzEmotesEntities.XathzEmotes downloadedEmotes = Http.DeserializeJson<XathzEmotesEntities.XathzEmotes>(data);
                    Cache.AddOrUpdate($"xathz_emotes_search_{search}", downloadedEmotes, 60);
                    emotes = downloadedEmotes;
                }
            }

            if (emotes is null || emotes.Emotes is null || emotes.Emotes.Count == 0) {
                await BetterReplyAsync("No emotes were found.", parameters: search);
                return;
            }

            XathzEmotesEntities.Emote randomEmote = emotes.Emotes.PickRandom();
            string[] randomEmoteNames = randomEmote.Names.Split(",");

            string names;
            if (randomEmoteNames.Length == 1) {
                names = $"**{randomEmoteNames[0]}**";
            } else {
                names = $"**{randomEmoteNames[0]}** aka: {string.Join(", ", randomEmoteNames.Skip(1).ToArray())}";
            }

            await BetterReplyAsync($"{names}\nhttps://emotes.xathz.net/files/{randomEmote.File}", parameters: search);
        }

        [SlashCommand("react", "React to the last 10 messages")]
        public async Task React([Summary("emote", "Which emote to react with")] Reaction emote) {
            ulong emoteId = emote == Reaction.AllThis ? 451081265467359253UL : 329534564160765954UL;

            if (!(Context.Guild.Emotes.FirstOrDefault(x => x.Id == emoteId) is IEmote guildEmote)) {
                await EphemeralReplyAsync("That emote no longer exists.");
                return;
            }

            await DeferAsync(true);

            IEnumerable<IMessage> messages = await Context.Channel.GetMessagesAsync(10).FlattenAsync();
            foreach (IUserMessage message in messages.OfType<IUserMessage>()) {
                await message.AddReactionAsync(guildEmote);
            }

            TrackStatistics(command: emote.ToString());
            await EphemeralReplyAsync("Done.");
        }

        [SlashCommand("bong", "Post a bong trap")]
        public async Task BongTrap() {
            IUserMessage message = await Context.Channel.SendMessageAsync("Bong!");
            DiscordClient.BongTrapMessageId = message.Id;
            await message.AddReactionAsync(new Emoji("⏰"));

            TrackStatistics(message.Id);
            await EphemeralReplyAsync("Done.");
        }

    }

}
