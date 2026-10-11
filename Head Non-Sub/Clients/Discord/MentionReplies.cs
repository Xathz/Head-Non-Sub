using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using HeadNonSub.Extensions;
using HeadNonSub.Settings;
using HeadNonSub.Statistics;

namespace HeadNonSub.Clients.Discord {

    /// <summary>
    /// Conversational replies when the bot is mentioned, like "@bot good bot".
    /// </summary>
    public class MentionReplies {

        private static readonly HashSet<string> _YouSuckPhrases = new HashSet<string> { "you suck", "i hate you", "fuck you", "fuck off", "die" };
        private static readonly HashSet<string> _YouAreGreatPhrases = new HashSet<string> { "you are great", "i love you", "how are you", "nice to see you", "i like you" };
        private static readonly HashSet<string> _BadBotPhrases = new HashSet<string> { "bad bot", "trash bot" };
        private static readonly HashSet<string> _GoodBotPhrases = new HashSet<string> { "good bot" };

        private static readonly char[] _TrimChars = new char[] { ' ', '.', '!', '?' };

        private readonly DiscordSocketClient _DiscordClient;

        public MentionReplies(DiscordSocketClient discordClient) {
            _DiscordClient = discordClient;
            _DiscordClient.MessageReceived += MessageReceived;
        }

        private Task MessageReceived(SocketMessage socketMessage) {
            if (!(socketMessage is SocketUserMessage message)) { return Task.CompletedTask; }
            if (message.Source != MessageSource.User) { return Task.CompletedTask; }
            if (!(message.Channel is SocketTextChannel channel)) { return Task.CompletedTask; }

            string text = RemoveMentionPrefix(message.Content);
            if (text is null) { return Task.CompletedTask; }

#if DEBUG
            if (message.Author.Id != Constants.XathzUserId) { return Task.CompletedTask; }
#endif

            if (SettingsManager.Configuration.DiscordBlacklist.Any(x => x.Key == channel.Guild.Id & x.Value.Contains(message.Author.Id))) {
                return Task.CompletedTask;
            }

            (string command, string reply) = GetReply(text.Trim(_TrimChars).ToLowerInvariant(), message.Author, channel.Guild);
            if (reply is null) { return Task.CompletedTask; }

            // Reply off the gateway thread
            _ = Task.Run(async () => {
                try {
                    IUserMessage sentMessage = await channel.SendMessageAsync(reply, allowedMentions: DiscordClient.UserMentionsOnly);

                    string nickname = message.Author is SocketGuildUser guildUser ? guildUser.Nickname ?? "" : "";
                    StatisticsManager.InsertCommand(message.CreatedAt.UtcDateTime, channel.Guild.Id, channel.Id, message.Author.Id, message.Author.ToString(), nickname,
                        message.Id, message.Content, command, "", sentMessage.Id);
                } catch (Exception ex) {
                    LoggingManager.Log.Error(ex);
                }
            });

            return Task.CompletedTask;
        }

        /// <summary>
        /// Get the text after a leading mention of the bot.
        /// </summary>
        /// <returns>Text after the mention; <see langword="null"/> if the message does not start with a mention of the bot.</returns>
        private string RemoveMentionPrefix(string content) {
            ulong id = _DiscordClient.CurrentUser.Id;

            foreach (string prefix in new string[] { $"<@{id}>", $"<@!{id}>" }) {
                if (content.StartsWith(prefix, StringComparison.Ordinal)) {
                    return content.Substring(prefix.Length);
                }
            }

            return null;
        }

        private static (string Command, string Reply) GetReply(string text, IUser author, SocketGuild guild) {
            if (_YouSuckPhrases.Contains(text)) {
                return ("YouSuck", new List<string> {
                    "i am poor :cry:",
                    ":open_mouth: y tho",
                    "I wonder how many racist things I can say before the mods ban me.",
                    "I was being purposefully retarded",
                    "listen here fucko",
                    "oof",
                    "PLS unban me from discord I am sorry for sending that stupid word art, also this song was spammed a lot in 2009",
                    "1024 warned me i am leving this place",
                    $"Alright, {guild.CurrentUser.Mention} has been warned for '**Being a useless person**'... wait, what?",
                    "but its F E E D I N G T I M E",
                    "Nani the FUCK?",
                    "ok?",
                    "I'm subbed on youtube tho",
                    "HEY WHY THE FUCK WAS I HACKED AND WHY THE FUCK DID SOMEONE BUY 50 BUCKS WORTH OF SUBS TO WUBBY"
                }.PickRandom());
            }

            if (_YouAreGreatPhrases.Contains(text)) {
                return ("YouAreGreat", new List<string> {
                    "awww",
                    ":heart:",
                    "I hope all subs are as nice as you!",
                    "camryn might be 14, but she is 18 in my heart",
                    "thats so sweet",
                    "Thats really nice but please give me a sub",
                    "d'awww",
                    "people are much nicer here than the ninja discord",
                    "do you think wubby could still love me as a non-sub?",
                    "they'll find my midget porn collection before they find weeb shit on my pc"
                }.PickRandom());
            }

            if (_BadBotPhrases.Contains(text)) {
                return ("BadBot", new List<string> {
                    "No u.",
                    $"I'm telling <@{Constants.XathzUserId}>!"
                }.PickRandom());
            }

            if (_GoodBotPhrases.Contains(text) && author.Id == Constants.XathzUserId) {
                return ("GoodBot", new List<string> {
                    "Damn right.",
                    "Yea, fuck them.",
                    $"<@{Constants.XathzUserId}> thanks for making me!",
                    "Yea everyone else sucks."
                }.PickRandom());
            }

            return (null, null);
        }

    }

}
