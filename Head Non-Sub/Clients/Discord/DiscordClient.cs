using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using HeadNonSub.Entities.Discord;
using HeadNonSub.Extensions;
using HeadNonSub.Settings;
using HeadNonSub.Statistics;
using Humanizer;
using Microsoft.Extensions.DependencyInjection;

namespace HeadNonSub.Clients.Discord {

    public static class DiscordClient {

        private static DiscordSocketConfig _DiscordConfig;
        private static DiscordSocketClient _DiscordClient;

        private static readonly CommandServiceConfig _ServiceConfig = new CommandServiceConfig() {
            DefaultRunMode = RunMode.Async, LogLevel = LogSeverity.Verbose
        };

        private static IServiceProvider _MentionProvider;
        private static readonly CommandService _MentionService = new CommandService(_ServiceConfig);

        private static IServiceProvider _ExclamationProvider;
        private static readonly CommandService _ExclamationService = new CommandService(_ServiceConfig);

        private static readonly List<string> _ValidPolls = new List<string>() { "poll:", "!strawpoll", "!strawpollresults", "!strawpollr", "!trashpoll" };

        public static async Task ConnectAsync() {
            _DiscordConfig = new DiscordSocketConfig {
                DefaultRetryMode = RetryMode.AlwaysRetry,
                ExclusiveBulkDelete = true,
                MessageCacheSize = 2500,
                AlwaysDownloadUsers = true,
                LogLevel = LogSeverity.Info
            };

            _DiscordClient = new DiscordSocketClient(_DiscordConfig);

            _MentionProvider = new ServiceCollection().AddSingleton(_DiscordClient)
                .AddSingleton(_MentionService)
                .AddSingleton<MentionCommands>()
                .BuildServiceProvider();

            _ExclamationProvider = new ServiceCollection().AddSingleton(_DiscordClient)
                .AddSingleton(_ExclamationService)
                .AddSingleton<ExclamationCommands>()
                .BuildServiceProvider();

            _DiscordClient.Log += Log;
            _DiscordClient.Connected += Connected;

            _DiscordClient.JoinedGuild += JoinedGuild;
            _DiscordClient.GuildAvailable += GuildAvailable;
            _DiscordClient.GuildMembersDownloaded += GuildMembersDownloaded;

            _DiscordClient.UserUpdated += UserUpdated;
            _DiscordClient.GuildMemberUpdated += GuildMemberUpdated;

            _DiscordClient.MessageReceived += MessageReceived;
            _DiscordClient.MessageUpdated += MessageUpdated;
            //_DiscordClient.MessageDeleted += MessageDeleted;

            _DiscordClient.ReactionAdded += ReactionAdded;

            _MentionProvider.GetRequiredService<CommandService>().Log += Log;
            _ExclamationProvider.GetRequiredService<CommandService>().Log += Log;

            await _MentionProvider.GetRequiredService<MentionCommands>().InitializeAsync();
            MentionCommandList = _MentionProvider.GetRequiredService<MentionCommands>().CommandList;

            await _ExclamationProvider.GetRequiredService<ExclamationCommands>().InitializeAsync();
            ExclamationCommandList = _ExclamationProvider.GetRequiredService<ExclamationCommands>().CommandList;

            await _DiscordClient.LoginAsync(TokenType.Bot, SettingsManager.Configuration.DiscordToken);
            await _DiscordClient.StartAsync();
        }

        public static ReadOnlyCollection<CommandInfo> MentionCommandList { get; private set; }

        public static ReadOnlyCollection<CommandInfo> ExclamationCommandList { get; private set; }

        public static async Task StopAsync() => await _DiscordClient.StopAsync();

        public static async void FailFast() {
            try {
                File.WriteAllText(Constants.FailFastFile, $"{DateTime.UtcNow.ToString("o")} This file was generated becuase the fail fast command was executed." +
                     " The watchdog script (Watchdog.ps1) will not start/restart the bot if this file is here.");

            } catch (Exception ex) {
                LoggingManager.Log.Error(ex);
            }

            await _DiscordClient.LogoutAsync();
            Environment.Exit(13);
        }

        public static async Task<ulong?> SendMessageAsync(ulong id, string message) {
            try {
                if (_DiscordClient.GetChannel(id) is IMessageChannel channel) {
                    return (await channel.SendMessageAsync(message)).Id;
                }

                if (_DiscordClient.GetUser(id) is IUser user) {
                    return (await user.SendMessageAsync(message)).Id;
                }

                return null;
            } catch (Exception ex) {
                LoggingManager.Log.Error(ex);
                return null;
            }
        }

        public static async Task TwitchChannelChange(ulong discordChannel, string streamUrl, string imageUrl, string title, string description, bool everyone = false) {
            try {
                if (_DiscordClient.GetChannel(discordChannel) is IMessageChannel channel) {

                    EmbedBuilder builder = new EmbedBuilder() {
                        Color = new Color(Constants.GeneralColor.R, Constants.GeneralColor.G, Constants.GeneralColor.B),
                        Url = streamUrl,
                        Title = title,
                        Description = description
                    };

                    if (imageUrl is string) {
                        builder.ImageUrl = $"{imageUrl.Replace("{width}", "1920").Replace("{height}", "1080")}?_={DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()}";
                    }

                    builder.Author = new EmbedAuthorBuilder {
                        Name = "Twitch",
                        IconUrl = Constants.TwitchLogoTransparentUrl,
                        Url = streamUrl
                    };

                    await channel.SendMessageAsync(text: (everyone ? $"@everyone" : null), embed: builder.Build());
                }
            } catch (Exception ex) {
                LoggingManager.Log.Error(ex);
            }
        }

        public static async Task SetStatus(string name = "", string url = "") {
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(url)) {
                await _DiscordClient.SetGameAsync(SettingsManager.Configuration.BotPlaying);
            } else {
                await _DiscordClient.SetGameAsync(name, url, ActivityType.Watching);
            }
        }

        private static Task Log(LogMessage message) {
            switch (message.Severity) {
                case LogSeverity.Debug:
                    LoggingManager.Log.Info(message.Message);
                    return Task.CompletedTask;

                case LogSeverity.Verbose:
                    LoggingManager.Log.Info(message.Message);
                    return Task.CompletedTask;

                case LogSeverity.Info:
                    LoggingManager.Log.Info(message.Message);
                    return Task.CompletedTask;

                case LogSeverity.Warning:
                    LoggingManager.Log.Warn(message.Message);
                    return Task.CompletedTask;

                case LogSeverity.Error:
                    LoggingManager.Log.Error(message.Message);
                    return Task.CompletedTask;

                case LogSeverity.Critical:
                    LoggingManager.Log.Fatal(message.Message);
                    return Task.CompletedTask;

                default:
                    LoggingManager.Log.Info($"UnknownSeverity: {message.Message}");
                    return Task.CompletedTask;
            }
        }

        private static async Task Connected() {
            try {
                await _DiscordClient.CurrentUser.ModifyAsync(x => x.Username = SettingsManager.Configuration.BotNickname);
                LoggingManager.Log.Info($"Changed the bot nickname to: {SettingsManager.Configuration.BotNickname}");

                await _DiscordClient.SetGameAsync(SettingsManager.Configuration.BotPlaying);
                LoggingManager.Log.Info($"Changed the bot 'now playing' status to: {SettingsManager.Configuration.BotPlaying}");

            } catch (Exception ex) {
                LoggingManager.Log.Error(ex);
            }
        }

        private static Task JoinedGuild(SocketGuild guild) {
            LoggingManager.Log.Warn($"Joined guild {guild.Name} ({guild.Id}).");

            // Leave if an invalid guild
            if (guild.Id != Constants.XathzDiscordGuild || guild.Id != WubbysFunHouse.ServerId) {
                guild.LeaveAsync(new RequestOptions { AuditLogReason = $"This bot is private. This guild needs to be whitelisted. Contact: {Constants.Creator}" });
            }

            return Task.CompletedTask;
        }

        private static Task GuildAvailable(SocketGuild guild) {
            LoggingManager.Log.Info($"Guild {guild.Name} ({guild.Id}) has become available");
            return Task.CompletedTask;
        }

        private static Task GuildMembersDownloaded(SocketGuild guild) {
            LoggingManager.Log.Info($"Full memberlist was downloaded for {guild.Name} ({guild.Id}); {guild.Users.Count.ToString("N0")} total members ({guild.Users.Where(x => x.Status == UserStatus.Online).Count().ToString("N0")} online)");
            return Task.CompletedTask;
        }

        private static Task UserUpdated(SocketUser oldUser, SocketUser newUser) {
            Task runner = Task.Run(async () => {
                await ProcessUserUpdated(oldUser, newUser).ConfigureAwait(false);
            });

            return Task.CompletedTask;
        }

        private static Task GuildMemberUpdated(SocketGuildUser oldUser, SocketGuildUser newUser) {
            Task runner = Task.Run(async () => {
                await ProcessUserUpdated(oldUser, newUser).ConfigureAwait(false);
            });

            return Task.CompletedTask;
        }

        private static async Task ProcessUserUpdated(IUser oldUser, IUser newUser) {
            try {
                StatisticsManager.NameChangeType changeType = StatisticsManager.NameChangeType.None;

                ulong? guildId = null;
                string oldUsername = null, newUsername = null;
                string oldDiscriminator = null, newDiscriminator = null;
                string oldNickname = null, newNickname = null;
                string backblazeAvatarBucket = null, backblazeAvatarFilename = null, backblazeAvatarUrl = null;

                if (oldUser is SocketGuildUser & newUser is SocketGuildUser) {
                    SocketGuildUser oldSocketUser = oldUser as SocketGuildUser;
                    SocketGuildUser newSocketUser = newUser as SocketGuildUser;

                    guildId = newSocketUser.Guild.Id;

                    if (oldSocketUser.Nickname != newSocketUser.Nickname) {
                        changeType |= StatisticsManager.NameChangeType.Display;

                        oldNickname = oldSocketUser.Nickname;
                        newNickname = newSocketUser.Nickname;
                    }
                }

                if (oldUser.Username != newUser.Username) {
                    changeType |= StatisticsManager.NameChangeType.Username;

                    oldUsername = oldUser.Username;
                    newUsername = newUser.Username;
                }

                if (oldUser.Discriminator != newUser.Discriminator) {
                    changeType |= StatisticsManager.NameChangeType.Discriminator;

                    oldDiscriminator = oldUser.Discriminator;
                    newDiscriminator = newUser.Discriminator;
                }

                if (oldUser.AvatarId != newUser.AvatarId) {
                    try {
                        ImageFormat imageFormat = newUser.AvatarId.StartsWith("a_") ? ImageFormat.Gif : ImageFormat.Png;
                        Task<MemoryStream> download = Http.GetStreamAsync(newUser.GetAvatarUrl(imageFormat, 1024));

                        using (MemoryStream data = await download) {
                            if (download.IsCompletedSuccessfully) {
                                Backblaze.File avatarUpload = await Backblaze.UploadAvatarAsync(data, $"{newUser.Id}/{Backblaze.ISOFileNameDate(imageFormat.ToString().ToLower())}");

                                backblazeAvatarBucket = avatarUpload.BucketName;
                                backblazeAvatarFilename = avatarUpload.FileName;
                                backblazeAvatarUrl = avatarUpload.ShortUrl;

                                changeType |= StatisticsManager.NameChangeType.Avatar;
                            } else {
                                LoggingManager.Log.Error(download.Exception);

                                backblazeAvatarBucket = null;
                                backblazeAvatarFilename = null;
                                backblazeAvatarUrl = null;
                            }
                        }
                    } catch { }
                }

                if (changeType != StatisticsManager.NameChangeType.None) {
                    StatisticsManager.InsertUserChange(DateTime.UtcNow, guildId, newUser.Id, changeType, oldUsername, newUsername, oldDiscriminator, newDiscriminator,
                        oldNickname, newNickname, backblazeAvatarBucket, backblazeAvatarFilename, backblazeAvatarUrl);
                }
            } catch (Exception ex) {
                LoggingManager.Log.Error(ex);
            }
        }

        private static async Task ReactionAdded(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel, SocketReaction reaction) {
            EmoteModeTracker.Mode emoteMode = EmoteModeTracker.GetMode(channel.Id);

            if (emoteMode == EmoteModeTracker.Mode.TextOnly) {
                if (message.HasValue & reaction.User.IsSpecified) {

                    if (WubbysFunHouse.IsDiscordOrTwitchStaff(reaction.User.Value)) {
                        return;
                    }

                    await message.Value.RemoveReactionAsync(reaction.Emote, reaction.User.Value);
                }
            }
        }

        private static async Task MessageReceived(SocketMessage socketMessage) {
            if (!(socketMessage is SocketUserMessage message)) { return; }
            if (socketMessage.Source != MessageSource.User) { return; }

            if (message.Channel is IPrivateChannel) {
                if (message.Author.Id == Constants.XathzUserId) {

                    try {
                        string channelId = message.Content.Split(' ')[0];
                        string messageText = message.Content.Replace(channelId, "").Trim();

                        if (!string.IsNullOrWhiteSpace(channelId) || !string.IsNullOrWhiteSpace(messageText)) {
                            ulong? reply = await SendMessageAsync(ulong.Parse(channelId), messageText);

                            if (reply.HasValue) {
                                await message.Channel.SendMessageAsync($":white_check_mark: Message sent.");
                            } else {
                                await message.Channel.SendMessageAsync(":warning: Message was not set.");
                            }
                        } else {
                            await message.Channel.SendMessageAsync(":information_source: Expected format: `<channelId> <message>`.");
                        }
                    } catch (Exception ex) {
                        await message.Channel.SendMessageAsync($":bangbang: Error: {ex.Message}");
                    }

                    return;
                } else {
                    await message.Channel.SendMessageAsync($"No commands are available via direct message. Please mention the bot (`@{_DiscordClient.CurrentUser.Username}`) on the server.");
                    return;
                }
            }

#if DEBUG
            if (message.Author.Id != Constants.XathzUserId) {
                return;
            }
#endif

            if (message.Author is SocketGuildUser user) {
                Task runner = Task.Run(async () => {
                    await ProcessMessageAsync(message, user).ConfigureAwait(false);
                });
            }
        }

        private static async Task MessageUpdated(Cacheable<IMessage, ulong> originalMessage, SocketMessage updatedMessage, ISocketMessageChannel channel) {
            if (!(updatedMessage is SocketUserMessage message)) { return; }
            if (updatedMessage.Source != MessageSource.User) { return; }

            if (WubbysFunHouse.IsDiscordOrTwitchStaff(updatedMessage.Author)) {
                return;
            }

            await ProcessMessageEmotesAsync(message);

            //string logMessage;

            //if (originalMessage.HasValue) {
            //    logMessage = $"`[{originalMessage.Value.CreatedAt.DateTime.ToUniversalTime().ToString(Constants.DateTimeFormatMedium)}]` :pencil: {originalMessage.Value.Author.ToString()} (`{originalMessage.Value.Author.Id}`) message edited in **#{channel.Name}**:{Environment.NewLine}**B:** {originalMessage.Value.Content}{Environment.NewLine}**A:** {updatedMessage.Content}";
            //} else {
            //    logMessage = $":warning: An unknown message was edited in **#{channel.Name}**: {Environment.NewLine}Old id was: {originalMessage.Id}{Environment.NewLine}New id is: {updatedMessage.Id}";
            //}

            //if (_DiscordClient.GetChannel(WubbysFunHouse.UserLogsChannelId) is IMessageChannel logChannel) {
            //    await logChannel.SendMessageAsync(logMessage);
            //}
        }

        //private static async Task MessageDeleted(Cacheable<IMessage, ulong> message, ISocketMessageChannel channel) {
        //    string logMessage;

        //    if (message.HasValue) {
        //        logMessage = $"`[{message.Value.CreatedAt.DateTime.ToUniversalTime().ToString(Constants.DateTimeFormatMedium)}]` :wastebasket: {message.Value.Author.ToString()} (`{message.Value.Author.Id}`) message deleted in **#{channel.Name}**:{Environment.NewLine}{message.Value.Content}";
        //    } else {
        //        logMessage = $":warning: An unknown message was deleted in **#{channel.Name}**: {Environment.NewLine}Message id was: {message.Id}";
        //    }

        //    if (_DiscordClient.GetChannel(WubbysFunHouse.UserLogsChannelId) is IMessageChannel logChannel) {
        //        await logChannel.SendMessageAsync(logMessage);
        //    }
        //}

        /// <summary>
        /// Process additional actions for a message.
        /// </summary>
        /// <param name="message">Message to process.</param>
        /// <param name="user">User who sent the message.</param>
        private static async Task ProcessMessageAsync(SocketUserMessage message, SocketGuildUser user) {

            if (WubbysFunHouse.IsDiscordOrTwitchStaff(user)) {
                return;
            }

            try {

                // Emote mode
                if (await ProcessMessageEmotesAsync(message)) {
                    return;
                }

                // Main channel
                if (message.Channel.Id == WubbysFunHouse.MainChannelId) {

                    // If not rank 10 or higher
                    if (!user.Roles.Any(x => x.Id == WubbysFunHouse.ForkliftDriversRoleId)) {

                        string betterUserFormat = $"{(string.IsNullOrWhiteSpace(user.Nickname) ? user.Username : user.Nickname)} `{user.ToString()}`";

                        // Move links
                        if (message.Content.ContainsUrls()) {
                            if (!message.Content.GetUrls().Any(x => x.Contains("https://discord.gift/"))) {
                                if (_DiscordClient.GetChannel(WubbysFunHouse.LinksChannelId) is IMessageChannel channel) {
                                    LoggingManager.Log.Info($"Link in #{message.Channel.Name} by {message.Author.ToString()} ({message.Author.Id})");

                                    await message.DeleteAsync();
                                    await channel.SendMessageAsync($"● Posted by {betterUserFormat} in <#{WubbysFunHouse.MainChannelId}>{Environment.NewLine}{message.Content}");
                                    await message.Channel.SendMessageAsync($"{user.Mention} You need to be <@&{WubbysFunHouse.ForkliftDriversRoleId}> or higher to post links here. Link was moved to <#{WubbysFunHouse.LinksChannelId}>.");
                                }
                            }

                            // Move attachments
                        } else if (message.Attachments.Count > 0) {
                            if (_DiscordClient.GetChannel(WubbysFunHouse.ActualFuckingSpamChannelId) is IMessageChannel channel) {
                                foreach (Attachment attachment in message.Attachments) {
                                    using (HttpResponseMessage response = await Http.Client.GetAsync(attachment.Url)) {
                                        if (response.IsSuccessStatusCode) {
                                            using (HttpContent content = response.Content) {
                                                Stream stream = await content.ReadAsStreamAsync();

                                                LoggingManager.Log.Info($"Attachment in #{message.Channel.Name} by {message.Author.ToString()} ({message.Author.Id}); {attachment.Filename}; api: {attachment.Size.Bytes().Humanize("#.##")}; downloaded: {stream.Length.Bytes().Humanize("#.##")}");

                                                if (stream.Length > Constants.DiscordMaximumFileSize) {
                                                    await channel.SendMessageAsync($"An attachment was uploaded by {betterUserFormat} in <#{WubbysFunHouse.MainChannelId}> and can not be re-uploaded, the attachment is too large for a bot to upload (`{attachment.Size.Bytes().Humanize("#.##")} / {stream.Length.Bytes().Humanize("#.##")}`). They probably have Nitro.");
                                                } else {
                                                    stream.Seek(0, SeekOrigin.Begin);
                                                    await channel.SendFileAsync(stream, attachment.Filename, $"● Uploaded by {betterUserFormat} in <#{WubbysFunHouse.MainChannelId}>{Environment.NewLine}{message.Content}");
                                                }
                                            }
                                        } else {
                                            throw new HttpRequestException($"There was an error; ({(int)response.StatusCode}) {response.ReasonPhrase}");
                                        }
                                    }
                                }

                                await message.DeleteAsync();
                                await message.Channel.SendMessageAsync($"{user.Mention} You need to be <@&{WubbysFunHouse.ForkliftDriversRoleId}> or higher to upload files here. Attachment was moved to <#{WubbysFunHouse.ActualFuckingSpamChannelId}>.");
                            }
                        }
                    }
                }

                // Invalid poll in market research
                if (message.Channel.Id == WubbysFunHouse.MarketResearchChannelId) {
                    string channelName = _DiscordClient.GetGuild(WubbysFunHouse.ServerId).GetChannel(WubbysFunHouse.MarketResearchChannelId).Name;

                    if (!_ValidPolls.Any(x => message.Content.Contains(x, StringComparison.OrdinalIgnoreCase))) {
                        LoggingManager.Log.Info($"Invalid poll in #{channelName} by {message.Author.ToString()} ({message.Author.Id})");

                        await message.DeleteAsync();
                        await message.Author.SendMessageAsync($"You can only post polls in #{channelName} on {_DiscordClient.GetGuild(WubbysFunHouse.ServerId).Name}.");
                    }
                }

                // Too many animated emotes in actual fucking spam
                if (message.Channel.Id == WubbysFunHouse.ActualFuckingSpamChannelId) {
                    int count = message.Content.CountStringOccurrences("<a:");

                    if (count > 10) {
                        await message.DeleteAsync();
                        await message.Channel.SendMessageAsync($"{user.Mention} Only 10 or less animated emotes/emoji per-message. Having many of them breaks the channel for some people.");
                    }
                }
            } catch (Exception ex) {
                LoggingManager.Log.Error(ex);
            }
        }

        /// <summary>
        /// Process emote mode for a message.
        /// </summary>
        /// <param name="message">Message to process.</param>
        /// <returns>True if message was deleted.</returns>
        private static async Task<bool> ProcessMessageEmotesAsync(SocketUserMessage message) {
            EmoteModeTracker.Mode emoteMode = EmoteModeTracker.GetMode(message.Channel.Id);

            if (emoteMode != EmoteModeTracker.Mode.Off) {

                if (message.Attachments.Count > 0) {
                    return await DeleteMessageAsync();
                }

                List<EmoteOrEmoji> emotes = message.Content.ParseDiscordMessageEmotes();

                if (emoteMode == EmoteModeTracker.Mode.TextOnly) {
                    if (emotes.Count > 0) {
                        return await DeleteMessageAsync();
                    }
                } else if (emoteMode == EmoteModeTracker.Mode.EmoteOnly) {
                    string content = message.Content;

                    foreach (EmoteOrEmoji emote in emotes) {
                        content = content.Replace(emote.ToString(), "");
                    }

                    if (!string.IsNullOrWhiteSpace(content)) {
                        return await DeleteMessageAsync();
                    }
                }

                async Task<bool> DeleteMessageAsync() {
                    await message.DeleteAsync();

                    if (EmoteModeTracker.ShouldSendNotification(message.Channel.Id)) {
                        await message.Channel.SendMessageAsync($"The channel is currently in **{emoteMode.Humanize().ToLower()}** mode.");
                    }

                    return true;
                }
            }

            return false;
        }

    }

}
