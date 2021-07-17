using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Rest;
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
            DefaultRunMode = RunMode.Async,
            LogLevel = LogSeverity.Verbose
        };

        private static IServiceProvider _MentionProvider;
        private static readonly CommandService _MentionService = new CommandService(_ServiceConfig);

        private static IServiceProvider _ExclamationProvider;
        private static readonly CommandService _ExclamationService = new CommandService(_ServiceConfig);

        public static ulong? BongTrapMessageId = null;

        public static async Task ConnectAsync() {
            _DiscordConfig = new DiscordSocketConfig {
                DefaultRetryMode = RetryMode.AlwaysRetry,
                ExclusiveBulkDelete = true,
                MessageCacheSize = 2500,
                AlwaysDownloadUsers = true,
#if DEBUG
                LogLevel = LogSeverity.Debug
#else
                LogLevel = LogSeverity.Info
#endif
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

            _DiscordClient.UserJoined += UserJoined;
            _DiscordClient.UserLeft += UserLeft;
            _DiscordClient.UserBanned += UserBanned;
            _DiscordClient.UserUpdated += UserUpdated;
            _DiscordClient.GuildMemberUpdated += GuildMemberUpdated;

            _DiscordClient.MessageReceived += MessageReceived;
            _DiscordClient.MessageUpdated += MessageUpdated;
            _DiscordClient.MessageDeleted += MessageDeleted;

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

        public static async Task TwitchChannelChange(ulong discordChannel, string streamUrl, string imageUrl, string title, string description, bool everyone = false, bool crosspost = false) {
            try {
                if (_DiscordClient.GetChannel(discordChannel) is IMessageChannel channel) {

                    EmbedBuilder builder = new EmbedBuilder() {
                        Color = new Color(Constants.GeneralColor.R, Constants.GeneralColor.G, Constants.GeneralColor.B),
                        Url = streamUrl,
                        Title = title
                    };

                    if (description is string) {
                        builder.Description = description;
                    }

                    if (imageUrl is string) {
                        builder.ImageUrl = $"{imageUrl.Replace("{width}", "1920").Replace("{height}", "1080")}?_={DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";
                    }

                    builder.Author = new EmbedAuthorBuilder {
                        Name = "Twitch",
                        IconUrl = Constants.TwitchLogoTransparentUrl,
                        Url = streamUrl
                    };

                    IUserMessage message = await channel.SendMessageAsync(text: (everyone ? $"@everyone" : null), embed: builder.Build());

                    if (crosspost == true) {
                        try {
                            await message.CrosspostAsync();
                        } catch { }
                    }

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
                    if (!message.Message.Contains("PRESENCE_UPDATE")) {
                        LoggingManager.Log.Debug(message.Message);
                    }
                    return Task.CompletedTask;

                case LogSeverity.Verbose:
                    LoggingManager.Log.Debug(message.Message);
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
            LoggingManager.Log.Info($"Full memberlist was downloaded for {guild.Name} ({guild.Id}); {guild.Users.Count:N0} total members ({guild.Users.Where(x => x.Status == UserStatus.Online).Count():N0} online)");
            return Task.CompletedTask;
        }

        private static async Task UserJoined(SocketGuildUser user) {

            // Log join
            if (_DiscordClient.GetChannel(WubbysFunHouse.UserLogsChannelId) is IMessageChannel logChannel) {
                await logChannel.SendMessageAsync($"`[{DateTime.UtcNow.ToString(Constants.DateTimeFormatMedium)}]` :inbox_tray: {user} (`{user.Id}`) has joined.");
            }

            // Username check
            if (!user.Username.All(x => x <= sbyte.MaxValue)) {
                _ = Task.Run(async () => {
                    await Task.Delay(1000);

                    try {
                        await WubbysFunHouse.AddRoleAsync(user, WubbysFunHouse.MutedRoleId, "Auto-muted on join due to weird username.");
                        await SendMessageAsync(WubbysFunHouse.ModLogsChannelId, $":exclamation: {user.Mention} was auto-muted on join due to due to weird username.");

                        await Task.Run(async () => {
                            await Task.Delay(8000);

                            if (!user.Roles.Any(x => x.Id == WubbysFunHouse.MutedRoleId)) {
                                await WubbysFunHouse.AddRoleAsync(user, WubbysFunHouse.MutedRoleId, "Auto-muted on join due to weird username (second attempt).");
                                await SendMessageAsync(WubbysFunHouse.ModLogsChannelId, $":bangbang: {user.Mention} was auto-muted on join due to due to weird username (second attempt, first one either failed or was overridden <@{Constants.XathzUserId}>).");
                            }
                        });
                    } catch (Exception ex) {
                        LoggingManager.Log.Error(ex);
                    }
                });
            }

            // Auto-mute accounts that have been created recently
            if (user.CreatedAt.AddDays(7) > DateTimeOffset.Now) {
                _ = Task.Run(async () => {
                    await Task.Delay(2000);

                    try {
                        await WubbysFunHouse.AddRoleAsync(user, WubbysFunHouse.MutedRoleId, "Auto-muted on join due to account being younger than a week.");
                        await SendMessageAsync(WubbysFunHouse.ModLogsChannelId, $":anger: {user.Mention} was auto-muted on join due to account being younger than a week.");

                    } catch (Exception ex) {
                        LoggingManager.Log.Error(ex);
                    }
                });
            }

            if (user == null) {
                LoggingManager.Log.Error("User joined, user is null");
                return;
            }

            if (string.IsNullOrEmpty(user.Guild.Id.ToString())) {
                LoggingManager.Log.Error("User joined, guild id is null");
                return;
            }

            await _DiscordClient.GetGuild(user.Guild.Id).DownloadUsersAsync();
            user = _DiscordClient.GetGuild(user.Guild.Id).GetUser(user.Id);

            if (string.IsNullOrEmpty(user.AvatarId)) {
                LoggingManager.Log.Error("User joined, avatar id is null!");
                return;
            }

            // ========

            Task runner = Task.Run(async () => {
                try {
                    StatisticsManager.NameChangeType changeType =
                        StatisticsManager.NameChangeType.Username |
                        StatisticsManager.NameChangeType.Discriminator |
                        StatisticsManager.NameChangeType.Display;

                    string backblazeAvatarBucket = null, backblazeAvatarFilename = null, backblazeAvatarUrl = null;

                    try {
                        ImageFormat imageFormat = user.AvatarId.StartsWith("a_") ? ImageFormat.Gif : ImageFormat.Png;
                        Task<MemoryStream> download = Http.GetStreamAsync(user.GetAvatarUrl(imageFormat, 1024));

                        using MemoryStream data = await download;
                        if (download.IsCompletedSuccessfully) {
                            Backblaze.File avatarUpload = await Backblaze.UploadAvatarAsync(data, $"{user.Id}/{Backblaze.ISOFileNameDate(imageFormat.ToString().ToLower())}");

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
                    } catch { }

                    StatisticsManager.InsertUserChange(DateTime.UtcNow, user.Guild.Id, user.Id, changeType, null, user.Username, null, user.Discriminator,
                        null, user.Nickname, backblazeAvatarBucket, backblazeAvatarFilename, backblazeAvatarUrl);

                } catch (Exception ex) {
                    LoggingManager.Log.Error(ex);
                }
            });
        }

        private static async Task UserLeft(SocketGuildUser user) {
            if (_DiscordClient.GetChannel(WubbysFunHouse.UserLogsChannelId) is IMessageChannel logChannel) {
                await logChannel.SendMessageAsync($"`[{DateTime.UtcNow.ToString(Constants.DateTimeFormatMedium)}]` :outbox_tray: {user} (`{user.Id}`) left the server.");
            }
        }

        private static async Task UserBanned(SocketUser user, SocketGuild guild) {
            if (_DiscordClient.GetChannel(WubbysFunHouse.UserLogsChannelId) is IMessageChannel userLogChannel) {
                await userLogChannel.SendMessageAsync($"`[{DateTime.UtcNow.ToString(Constants.DateTimeFormatMedium)}]` :no_entry: {user} (`{user.Id}`) was banned.");
            }

            try {
                // Lookup audit log event
                Task runner = Task.Run(async () => {
                    await Task.Delay(4000);

                    if (_DiscordClient.GetChannel(WubbysFunHouse.ModLogsChannelId) is IMessageChannel modLogChannel) {
                        IEnumerable<RestAuditLogEntry> userBans = await guild.GetAuditLogsAsync(10, actionType: ActionType.Ban).FlattenAsync();
                        RestAuditLogEntry bannedEvent = userBans.Where(x => x.Data is BanAuditLogData data && data.Target.Id == user.Id).FirstOrDefault();

                        if (string.IsNullOrEmpty(bannedEvent.Reason)) {
                            await modLogChannel.SendMessageAsync($":no_entry: {user} (`{user.Id}`) was banned by {bannedEvent.User} (`{bannedEvent.User.Id}`) with no reason specified.");
                        } else {
                            await modLogChannel.SendMessageAsync($":no_entry: {user} (`{user.Id}`) was banned by {bannedEvent.User} (`{bannedEvent.User.Id}`) with reason: **{bannedEvent.Reason}**");
                        }
                    }
                });
            } catch (Exception ex) {
                LoggingManager.Log.Error(ex);
            }
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

            // Bot check
            //try {
            //    if (WubbysFunHouse.IsServerBot(newUser.Id)) {
            //        if (oldUser.Status == UserStatus.Online & newUser.Status == UserStatus.Offline) {
            //            await SendMessageAsync(WubbysFunHouse.ModLogsChannelId, $":warning: The bot {newUser.Mention} is **offline**!");
            //        } else if (oldUser.Status == UserStatus.Offline & newUser.Status == UserStatus.Online) {
            //            await SendMessageAsync(WubbysFunHouse.ModLogsChannelId, $":white_check_mark: The bot {newUser.Mention} is back **online**.");
            //        }
            //    }
            //} catch (Exception ex) {
            //    LoggingManager.Log.Error(ex);
            //}

            // User changes
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

                        using MemoryStream data = await download;
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

            // BongBot
            //try {
            //    if (reaction.User.IsSpecified && reaction.User.Value.Id == 735232851431260290 && message.HasValue) {
            //        _ = message.Value.AddReactionAsync(reaction.Emote);
            //        LoggingManager.Log.Debug($"Reacted to BongBot {reaction.Emote}");
            //    }
            //} catch (Exception ex) {
            //    LoggingManager.Log.Error(ex);
            //}

            // Bong trap
            try {
                if (BongTrapMessageId.HasValue && message.Id == BongTrapMessageId && reaction.User.IsSpecified && reaction.User.Value is SocketGuildUser socketUser) {
                    Random rand = new Random();
                    if (!string.IsNullOrEmpty(socketUser.Nickname)) {
                        string scrambledNick = new string(socketUser.Nickname.ToCharArray().OrderBy(s => (rand.Next(2) % 2) == 0).ToArray());
                        _ = socketUser.ModifyAsync(x => x.Nickname = scrambledNick);
                    } else {
                        string scrambledUsername = new string(socketUser.Username.ToCharArray().OrderBy(s => (rand.Next(2) % 2) == 0).ToArray());
                        _ = socketUser.ModifyAsync(x => x.Nickname = scrambledUsername);
                    }
                }
            } catch (Exception ex) {
                LoggingManager.Log.Error(ex);
            }

            // Emote mode
            (EmoteModeTracker.Mode mode, object _) = EmoteModeTracker.GetMode(channel.Id);

            if (mode == EmoteModeTracker.Mode.TextOnly) {
                if (message.HasValue & reaction.User.IsSpecified) {

                    if (WubbysFunHouse.IsDiscordOrTwitchStaff(reaction.User.Value)) {
                        return;
                    }

                    await message.Value.RemoveReactionAsync(reaction.Emote, reaction.User.Value);
                }
            }
        }

        private static async Task MessageReceived(SocketMessage socketMessage) {

            #region Url in mod mail message

            if (socketMessage.Author.Id == WubbysFunHouse.WubbyMailBotUserId &&
                socketMessage.Channel is SocketTextChannel textChannel &&
                textChannel.CategoryId.HasValue &&
                textChannel.CategoryId.Value == WubbysFunHouse.ModMailCategoryId &&
                socketMessage.Embeds.Any(x => x.Description.ContainsUrls())) {

                List<string> urls = socketMessage.Embeds.FirstOrDefault().Description.GetUrls();
                await socketMessage.Channel.SendMessageAsync($"Url detected in `WubbyMail` message, attempting to embed...{Environment.NewLine}{string.Join(Environment.NewLine, urls)}");

                return;
            }

            #endregion

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

            if (message.Author is SocketGuildUser user && message.Channel is SocketTextChannel channel) {
                Task runner = Task.Run(async () => {
                    await ProcessMessageAsync(channel, message, user).ConfigureAwait(false);
                });
            }
        }

        private static async Task MessageUpdated(Cacheable<IMessage, ulong> originalMessage, SocketMessage updatedMessage, ISocketMessageChannel channel) {
            if (originalMessage.HasValue && !string.IsNullOrWhiteSpace(originalMessage.Value.Content) && !string.IsNullOrWhiteSpace(updatedMessage.Content) && !originalMessage.Value.Author.IsBot) {
                if (originalMessage.Value.Content != updatedMessage.Content) {
                    string logMessage;

                    if (originalMessage.HasValue) {
                        logMessage = $"`[{originalMessage.Value.CreatedAt.DateTime.ToUniversalTime().ToString(Constants.DateTimeFormatMedium)}]` :pencil: {originalMessage.Value.Author} (`{originalMessage.Value.Author.Id}`) message edited in **#{channel.Name}**:{Environment.NewLine}**B:** {originalMessage.Value.ResolveTags()}{Environment.NewLine}**A:** {updatedMessage.ResolveTags()}";
                    } else {
                        logMessage = $":warning: An unknown message was edited in **#{channel.Name}**: {Environment.NewLine}Old id was: {originalMessage.Id}{Environment.NewLine}New id is: {updatedMessage.Id}";
                    }

                    if (!string.IsNullOrEmpty(logMessage)) {
                        if (_DiscordClient.GetChannel(WubbysFunHouse.UserLogsChannelId) is IMessageChannel logChannel) {
                            await logChannel.SendMessageAsync(logMessage);
                        }
                    }
                }
            }

            // ========

            if (!(updatedMessage is SocketUserMessage message)) { return; }
            if (updatedMessage.Source != MessageSource.User) { return; }

            if (!WubbysFunHouse.IsDiscordOrTwitchStaff(updatedMessage.Author)) {
                await ProcessMessageEmotesAsync(message);
            }
        }

        private static async Task MessageDeleted(Cacheable<IMessage, ulong> message, ISocketMessageChannel channel) {

            // Bong trap
            if (message.Id == BongTrapMessageId) {
                BongTrapMessageId = null;
            }

            if (message.HasValue && !string.IsNullOrWhiteSpace(message.Value.Content) && !message.Value.Author.IsBot) {
                string logMessage;

                if (message.HasValue) {
                    logMessage = $"`[{message.Value.CreatedAt.DateTime.ToUniversalTime().ToString(Constants.DateTimeFormatMedium)}]` :wastebasket: {message.Value.Author} (`{message.Value.Author.Id}`) message deleted in **#{channel.Name}**:{Environment.NewLine}{message.Value.ResolveTags()}";
                } else {
                    logMessage = $":warning: An unknown message was deleted in **#{channel.Name}**: {Environment.NewLine}Message id was: {message.Id}";
                }

                if (_DiscordClient.GetChannel(WubbysFunHouse.UserLogsChannelId) is IMessageChannel logChannel) {
                    await logChannel.SendMessageAsync(logMessage);
                }
            }
        }

        /// <summary>
        /// Process additional actions for a message.
        /// </summary>
        /// <param name="channel">Text channel message was sent in.</param>
        /// <param name="message">Message to process.</param>
        /// <param name="user">User who sent the message.</param>
        private static async Task ProcessMessageAsync(SocketTextChannel channel, SocketUserMessage message, SocketGuildUser user) {

            if (WubbysFunHouse.IsDiscordOrTwitchStaff(user)) {
                return;
            }

            try {

                // chadwick (226992558839037952) #server-suggestions (502630201948373003) 50% chance
                //if (user.Id == 226992558839037952 && channel.Id == 502630201948373003 && new Random().Next(1, 100) > 50) {
                //    await channel.SendMessageAsync($"<@226992558839037952> No");
                //}

                // Emote mode
                if (await ProcessMessageEmotesAsync(message)) {
                    return;
                }

                // Jingle bells
                //if (message.Content.Replace(" ", "").Contains("jingle", StringComparison.OrdinalIgnoreCase)) {
                //    Task runner = Task.Run(async () => {
                //        await JingleBells(message.Channel.Id).ConfigureAwait(false);
                //    });
                //}

                string contentLowercase = message.Content.ToLowerInvariant();

                // Discord invites
                if (contentLowercase.Contains("https://discord.gg/")) {
                    await message.DeleteAsync();
                    await channel.SendMessageAsync($"{user.Mention} No invites allowed.");
                    return;
                }

                // Discord gifts
                if (contentLowercase.Contains("https://discord.gift/") && WubbysFunHouse.IsSubscriber(user)) {
                    return;
                }

                // Main channel
                if (channel.Id == WubbysFunHouse.MainChannelId) {

                    // If tier 3 skip filters
                    if (user.Roles.Any(x => x.Id == WubbysFunHouse.TwitchSubscriberTier3RoleId)) {
                        return;
                    }

                    // If not rank 10 or higher
                    if (!user.Roles.Any(x => x.Id == WubbysFunHouse.ForkliftDriversRoleId)) {

                        string betterUserFormat = $"{(string.IsNullOrWhiteSpace(user.Nickname) ? user.Username : user.Nickname)} `{user}`";

                        // Move links
                        if (message.Content.ContainsUrls()) {
                            if (_DiscordClient.GetChannel(WubbysFunHouse.LinksChannelId) is IMessageChannel linksChannel) {
                                LoggingManager.Log.Info($"Link in #{channel.Name} by {user} ({user.Id})");

                                await linksChannel.SendMessageAsync($"● Posted by {betterUserFormat} in <#{WubbysFunHouse.MainChannelId}>{Environment.NewLine}{message.ResolveTags()}");

                                await message.DeleteAsync();
                                await channel.SendMessageAsync($"{user.Mention} You need the `Forklift Drivers` role or higher to post links here. Link was moved to <#{WubbysFunHouse.LinksChannelId}>.");
                            }

                            // Move attachments
                        } else if (message.Attachments.Count > 0) {
                            if (_DiscordClient.GetChannel(WubbysFunHouse.ActualFuckingSpamChannelId) is IMessageChannel actualFuckingSpamChannel) {
                                foreach (Attachment attachment in message.Attachments) {
                                    using HttpResponseMessage response = await Http.Client.GetAsync(attachment.Url);

                                    if (response.IsSuccessStatusCode) {
                                        using HttpContent content = response.Content;
                                        Stream stream = await content.ReadAsStreamAsync();

                                        LoggingManager.Log.Info($"Attachment in #{channel.Name} by {user} ({user.Id}); {attachment.Filename}; api: {attachment.Size.Bytes().Humanize("#.##")}; downloaded: {stream.Length.Bytes().Humanize("#.##")}");

                                        if (stream.Length > Constants.DiscordMaximumFileSize) {
                                            await actualFuckingSpamChannel.SendMessageAsync($"An attachment was uploaded by {betterUserFormat} in <#{WubbysFunHouse.MainChannelId}> and can not be re-uploaded, the attachment is too large for a bot to upload (`{attachment.Size.Bytes().Humanize("#.##")} / {stream.Length.Bytes().Humanize("#.##")}`). They probably have Nitro.");
                                        } else {
                                            stream.Seek(0, SeekOrigin.Begin);
                                            await actualFuckingSpamChannel.SendFileAsync(stream, attachment.Filename, $"● Uploaded by {betterUserFormat} in <#{WubbysFunHouse.MainChannelId}>{Environment.NewLine}{message.Content}");
                                        }
                                    } else {
                                        throw new HttpRequestException($"There was an error; ({(int)response.StatusCode}) {response.ReasonPhrase}");
                                    }
                                }

                                await message.DeleteAsync();
                                await channel.SendMessageAsync($"{user.Mention} You need to be `Forklift Drivers` or higher to upload files here. Attachment was moved to <#{WubbysFunHouse.ActualFuckingSpamChannelId}>.");
                            }
                        }
                    }
                }

                // Too many animated emotes in actual fucking spam
                if (channel.Id == WubbysFunHouse.ActualFuckingSpamChannelId) {
                    int count = message.Content.CountStringOccurrences("<a:");

                    if (count > 10) {
                        await message.DeleteAsync();
                        await channel.SendMessageAsync($"{user.Mention} Only 10 or less animated emotes/emoji per-message. Having many of them breaks the channel for some people.");
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
            (EmoteModeTracker.Mode mode, object args) = EmoteModeTracker.GetMode(message.Channel.Id);

            if (mode != EmoteModeTracker.Mode.Off) {

                if (message.Attachments.Count > 0) {
                    return await DeleteMessageAsync();
                }

                List<EmoteOrEmoji> emotes = message.Content.ParseDiscordMessageEmotes();

                if (mode == EmoteModeTracker.Mode.TextOnly) {
                    if (emotes.Count > 0) {
                        return await DeleteMessageAsync();
                    }

                } else if (mode == EmoteModeTracker.Mode.EmoteOnly) {
                    string content = message.Content;

                    foreach (EmoteOrEmoji emote in emotes) {
                        content = content.Replace(emote.ToString(), "");
                    }

                    if (!string.IsNullOrWhiteSpace(content)) {
                        return await DeleteMessageAsync();
                    }

                } else if (mode == EmoteModeTracker.Mode.Exactly) {
                    if (args is string argString) {
                        if (!string.Equals(message.Content, argString, StringComparison.InvariantCultureIgnoreCase)) {
                            return await DeleteMessageAsync();
                        }
                    }

                }

                async Task<bool> DeleteMessageAsync() {
                    await message.DeleteAsync();

                    if (EmoteModeTracker.ShouldSendNotification(message.Channel.Id)) {
                        await message.Channel.SendMessageAsync($"The channel is currently in **{mode.Humanize().ToLower()}** mode.");
                    }

                    return true;
                }
            }

            return false;
        }

        //private static DateTimeOffset _LastJingleBells = DateTimeOffset.UtcNow;
        //private static bool _SentJingleBellsCooldownMessage = false;

        /// <summary>
        /// Jingles the bells.
        /// </summary>
        //private static async Task JingleBells(ulong channelId) {
        //    if (_DiscordClient.GetChannel(channelId) is IMessageChannel channel) {
        //        if (channelId == WubbysFunHouse.ActualFuckingSpamChannelId) { return; }

        //        if (_LastJingleBells.AddSeconds(240) >= DateTimeOffset.UtcNow) {
        //            if (!_SentJingleBellsCooldownMessage) {
        //                _SentJingleBellsCooldownMessage = true;

        //                string remaining = (_LastJingleBells.AddSeconds(240) - DateTimeOffset.UtcNow).TotalSeconds.Seconds().Humanize(2);
        //                await channel.SendMessageAsync($":mrs_claus::skin-tone-5: The elves are sleepy! Check back in {remaining} for some more cheer.");
        //            }
        //            return;
        //        }

        //        _LastJingleBells = DateTimeOffset.UtcNow;
        //        _SentJingleBellsCooldownMessage = false;

        //        int msDelay = 2000;
        //        string dashing = $"{Environment.NewLine}Dashing";

        //        IUserMessage sentMessage = await channel.SendMessageAsync($":wave::skin-tone-5: _I'm slow so bear with me..._");
        //        await Task.Delay(2600);

        //        await sentMessage.ModifyAsync(x => { x.Content = $":snowman2:{dashing}"; });
        //        await Task.Delay(msDelay);

        //        await sentMessage.ModifyAsync(x => { x.Content = $":snowman:{dashing} through"; });
        //        await Task.Delay(msDelay);

        //        await sentMessage.ModifyAsync(x => { x.Content = $":snowman2:{dashing} through the"; });
        //        await Task.Delay(msDelay);

        //        await sentMessage.ModifyAsync(x => { x.Content = $":snowman:{dashing} through the snow"; });
        //        await Task.Delay(msDelay);

        //        await sentMessage.ModifyAsync(x => { x.Content = $":snowman2:{dashing} through the snow{Environment.NewLine}In"; });
        //        await Task.Delay(msDelay);

        //        await sentMessage.ModifyAsync(x => { x.Content = $":snowman:{dashing} through the snow{Environment.NewLine}In a"; });
        //        await Task.Delay(msDelay);

        //        await sentMessage.ModifyAsync(x => { x.Content = $":snowman2:{dashing} through the snow{Environment.NewLine}In a one"; });
        //        await Task.Delay(msDelay);

        //        await sentMessage.ModifyAsync(x => { x.Content = $":snowman:{dashing} through the snow{Environment.NewLine}In a one-horse"; });
        //        await Task.Delay(msDelay);

        //        await sentMessage.ModifyAsync(x => { x.Content = $":snowman2:{dashing} through the snow{Environment.NewLine}In a one-horse open"; });
        //        await Task.Delay(msDelay);

        //        await sentMessage.ModifyAsync(x => { x.Content = $":snowman:{dashing} through the snow{Environment.NewLine}In a one-horse open sleigh"; });
        //        await Task.Delay(msDelay);

        //        await sentMessage.ModifyAsync(x => { x.Content = $":cloud_snow:{dashing} through the snow{Environment.NewLine}In a one-horse open sleigh{Environment.NewLine}O'er"; });
        //        await Task.Delay(msDelay);

        //        await sentMessage.ModifyAsync(x => { x.Content = $":snowflake:{dashing} through the snow{Environment.NewLine}In a one-horse open sleigh{Environment.NewLine}O'er the"; });
        //        await Task.Delay(msDelay);

        //        await sentMessage.ModifyAsync(x => { x.Content = $":cloud_snow:{dashing} through the snow{Environment.NewLine}In a one-horse open sleigh{Environment.NewLine}O'er the fields"; });
        //        await Task.Delay(msDelay);

        //        await sentMessage.ModifyAsync(x => { x.Content = $":snowflake:{dashing} through the snow{Environment.NewLine}In a one-horse open sleigh{Environment.NewLine}O'er the fields we"; });
        //        await Task.Delay(msDelay);

        //        await sentMessage.ModifyAsync(x => { x.Content = $":cloud_snow:{dashing} through the snow{Environment.NewLine}In a one-horse open sleigh{Environment.NewLine}O'er the fields we go"; });
        //        await Task.Delay(msDelay);

        //        await sentMessage.ModifyAsync(x => { x.Content = $":laughing:{dashing} through the snow{Environment.NewLine}In a one-horse open sleigh{Environment.NewLine}O'er the fields we go{Environment.NewLine}Laughing"; });
        //        await Task.Delay(msDelay);

        //        await sentMessage.ModifyAsync(x => { x.Content = $":rofl:{dashing} through the snow{Environment.NewLine}In a one-horse open sleigh{Environment.NewLine}O'er the fields we go{Environment.NewLine}Laughing all"; });
        //        await Task.Delay(msDelay);

        //        await sentMessage.ModifyAsync(x => { x.Content = $":laughing:{dashing} through the snow{Environment.NewLine}In a one-horse open sleigh{Environment.NewLine}O'er the fields we go{Environment.NewLine}Laughing all the"; });
        //        await Task.Delay(msDelay);

        //        await sentMessage.ModifyAsync(x => { x.Content = $":rofl:{dashing} through the snow{Environment.NewLine}In a one-horse open sleigh{Environment.NewLine}O'er the fields we go{Environment.NewLine}Laughing all the way"; });
        //        await Task.Delay(4200);

        //        await sentMessage.ModifyAsync(x => { x.Content = $":christmas_tree: :candle: **Happy holidays** :menorah: :star2:"; });
        //    }
        //}

    }

}
