using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using HeadNonSub.Clients.Discord.Attributes;
using HeadNonSub.Extensions;

namespace HeadNonSub.Clients.Discord.Commands.Exclamation {

    [BlacklistEnforced]
    [RequireContext(ContextType.Guild)]
    public class Rythm : BetterModuleBase {

        [Command("randomsong")]
        public async Task RandomSong() {
            IMessage randomMessage = null;

            // 'bot-commands'
            if (Context.Guild.Channels.Where(x => x.Id == 462517221789532170).FirstOrDefault() is SocketTextChannel channel) {
                IAsyncEnumerable<IMessage> messages = channel.GetMessagesAsync(500).Flatten();

                // 'Rythm#3722' or 'Rythm 2#2000'
                IAsyncEnumerable<IMessage> validMessages = messages
                    .Where(x => x.Author.IsBot & (x.Author.Id == 235088799074484224 || x.Author.Id == 252128902418268161))
                    .Where(x => x.Embeds.Any(e => e.Author.HasValue && e.Author.Value.Name.Contains("Added to queue")));

                randomMessage = (await validMessages.ToListAsync()).PickRandom();
            }

            if (randomMessage is IMessage) {
                EmbedBuilder builder = new EmbedBuilder() {
                    Color = new Color(Constants.GeneralColor.R, Constants.GeneralColor.G, Constants.GeneralColor.B),
                    Description = $"{randomMessage.Embeds.ElementAt(0).Description}{Environment.NewLine}**Duration:** {randomMessage.Embeds.ElementAt(0).Fields.Where(x => x.Name == "Song Duration").FirstOrDefault().Value}",
                    ThumbnailUrl = randomMessage.Embeds.ElementAt(0).Thumbnail.Value.Url
                };

                builder.Author = new EmbedAuthorBuilder() {
                    IconUrl = randomMessage.Author.GetAvatarUrl(),
                    Name = $"Random Song from {randomMessage.Author.Username}"
                };

                IUserMessage reply = await BetterReplyAsync(embed: builder.Build());

                try {
                    IEmote upvotepost = Context.Guild.Emotes.FirstOrDefault(x => x.Id == 529559392157171731);
                    IEmote downvotepost = Context.Guild.Emotes.FirstOrDefault(x => x.Id == 529560241751457803);

                    await reply.AddReactionAsync(upvotepost);
                    await reply.AddReactionAsync(downvotepost);
                } catch { }
            } else {
                await BetterReplyAsync("Failed to pick a random song.");
            }

        }

    }

}
