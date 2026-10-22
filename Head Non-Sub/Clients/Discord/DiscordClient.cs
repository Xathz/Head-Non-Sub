using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Discord.Net;
using Discord.Rest;
using Discord.WebSocket;
using HeadNonSub.Extensions;
using HeadNonSub.Settings;
using Humanizer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace HeadNonSub.Clients.Discord {

    public static class DiscordClient {

        private static DiscordSocketConfig _DiscordConfig;
        private static DiscordSocketClient _DiscordClient;

        /// <summary>
        /// Only allow user mentions, blocks @everyone, @here and role pings from user supplied text.
        /// </summary>
        public static readonly AllowedMentions UserMentionsOnly = new AllowedMentions(AllowedMentionTypes.Users);

        private static IServiceProvider _CommandProvider;

        private static readonly TimeSpan _ReconnectTimeout = TimeSpan.FromMinutes(10);
        private static DateTimeOffset _LastConnected = DateTimeOffset.MinValue;
        private static bool _Stopping;

        private static readonly Regex _InviteRegex = new Regex(@"(?:discord\.gg|discord(?:app)?\.com/invite)/[\w-]+", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <summary>
        /// Optional IOptions configuration provider (for dependency injection).
        /// </summary>
        private static IOptions<Configuration> _ConfigurationOptions;

        public static ulong? BongTrapMessageId = null;

        /// <summary>
        /// Set the configuration options (called during startup from DI container).
        /// </summary>
        public static void SetConfigurationOptions(IOptions<Configuration> configurationOptions) => _ConfigurationOptions = configurationOptions;

        /// <summary>
        /// Get the current configuration, preferring injected options over static SettingsManager.
        /// </summary>
        private static Configuration GetConfiguration() {
            if (_ConfigurationOptions != null) {
                return _ConfigurationOptions.Value;
            }
            return SettingsManager.Configuration;
        }

        public static async Task ConnectAsync() {
            _DiscordConfig = new DiscordSocketConfig {
                // Only what the bot uses, members and message content are privileged and must be enabled in the developer portal
                GatewayIntents = GatewayIntents.Guilds | GatewayIntents.GuildMembers | GatewayIntents.GuildBans | GatewayIntents.GuildEmojis |
                    GatewayIntents.GuildMessages | GatewayIntents.GuildMessageReactions | GatewayIntents.DirectMessages | GatewayIntents.MessageContent,
                DefaultRetryMode = RetryMode.AlwaysRetry,
                MessageCacheSize = 3000,
                AlwaysDownloadUsers = true,
#if DEBUG
                LogLevel = LogSeverity.Debug
#else
                LogLevel = LogSeverity.Info
#endif
            };

            _DiscordClient = new DiscordSocketClient(_DiscordConfig);

            InteractionServiceConfig interactionConfig = new InteractionServiceConfig() {
                DefaultRunMode = RunMode.Async,
                LogLevel = LogSeverity.Verbose
            };

            _CommandProvider = new ServiceCollection().AddSingleton(_DiscordClient)
                .AddSingleton(x => new InteractionService(_DiscordClient.Rest, interactionConfig))
                .AddSingleton<SlashCommands>()
                .BuildServiceProvider();

            // Subscribes to message events for @bot replies
            new MentionReplies(_DiscordClient);

            _DiscordClient.Log += Log;
            _DiscordClient.Connected += Connected;
            _DiscordClient.Disconnected += Disconnected;

            _DiscordClient.JoinedGuild += JoinedGuild;
            _DiscordClient.GuildAvailable += GuildAvailable;
            _DiscordClient.GuildMembersDownloaded += GuildMembersDownloaded;

            _DiscordClient.UserJoined += UserJoined;
            _DiscordClient.UserLeft += UserLeft;
            _DiscordClient.UserBanned += UserBanned;

            _DiscordClient.MessageReceived += MessageReceived;
            _DiscordClient.MessageUpdated += MessageUpdated;
            _DiscordClient.MessageDeleted += MessageDeleted;

            _DiscordClient.ReactionAdded += ReactionAdded;

            _CommandProvider.GetRequiredService<InteractionService>().Log += Log;
            await _CommandProvider.GetRequiredService<SlashCommands>().InitializeAsync();

            // Subscribes to message events for ! aliases, after the slash commands are loaded so it can find them
            new PrefixCommands(_CommandProvider);

            await _DiscordClient.LoginAsync(TokenType.Bot, GetConfiguration().DiscordToken);
            await _DiscordClient.StartAsync();
        }

        public static async Task StopAsync() {
            _Stopping = true;

            // Not created when startup failed before connecting
            if (_DiscordClient != null) {
                await _DiscordClient.StopAsync();
            }
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

        private static Task Log(LogMessage message) {
            NLog.LogLevel level = message.Severity switch {
                LogSeverity.Debug => NLog.LogLevel.Debug,
                LogSeverity.Verbose => NLog.LogLevel.Debug,
                LogSeverity.Info => NLog.LogLevel.Info,
                LogSeverity.Warning => NLog.LogLevel.Warn,
                LogSeverity.Error => NLog.LogLevel.Error,
                _ => NLog.LogLevel.Fatal
            };

            // Show where in Discord.Net the message came from, like Discord.Gateway
            string source = message.Source == "Discord" ? "Discord" : $"Discord.{message.Source.Replace(" ", "")}";
            LoggingManager.Log.WithProperty("Source", source).Log(level, message.Exception, message.Message ?? string.Empty);
            return Task.CompletedTask;
        }

        private static async Task Connected() {
            _LastConnected = DateTimeOffset.UtcNow;

            try {
                // Discord only allows a couple of username changes per hour
                if (_DiscordClient.CurrentUser.Username != GetConfiguration().BotNickname) {
                    await _DiscordClient.CurrentUser.ModifyAsync(x => x.Username = GetConfiguration().BotNickname);
                    LoggingManager.Log.Info($"Changed the bot username to {GetConfiguration().BotNickname}");
                }

                await _DiscordClient.SetGameAsync(GetConfiguration().BotPlaying);
                LoggingManager.Log.Info($"Set the bot status to playing {GetConfiguration().BotPlaying}");

            } catch (Exception ex) {
                LoggingManager.Log.Error(ex);
            }
        }

        private static Task Disconnected(Exception exception) {
            if (_Stopping) { return Task.CompletedTask; }

            // Discord.Net keeps retrying these or stops silently, neither works until the cause is fixed
            string fatalReason = GetFatalDisconnectReason(exception);
            if (fatalReason != null) {
                Shutdown.FailFast(fatalReason);
                return Task.CompletedTask;
            }

            // Some closes make Discord.Net stop reconnecting, restart if it has not reconnected after a while
            DateTimeOffset disconnectedAt = DateTimeOffset.UtcNow;
            _ = Task.Run(async () => {
                await Task.Delay(_ReconnectTimeout);

                if (!_Stopping && _LastConnected < disconnectedAt) {
                    Shutdown.Restart($"Not reconnected to Discord {_ReconnectTimeout.TotalMinutes} minutes after disconnecting");
                }
            });

            return Task.CompletedTask;
        }

        /// <summary>
        /// Get why a disconnect can not be fixed by reconnecting.
        /// </summary>
        /// <param name="exception">Exception Discord.Net disconnected with.</param>
        /// <returns>What needs to be fixed; <see langword="null"/> if reconnecting can work.</returns>
        public static string GetFatalDisconnectReason(Exception exception) {
            for (Exception current = exception; current != null; current = current.InnerException) {
                if (current is WebSocketClosedException closed) {
                    // https://discord.com/developers/docs/topics/opcodes-and-status-codes#gateway-gateway-close-event-codes
                    return closed.CloseCode switch {
                        4004 => "Discord rejected the token (close code 4004), check discordToken in the settings file",
                        4014 => "Discord refused the privileged intents (close code 4014), enable Server Members Intent and Message Content Intent in the Discord Developer Portal",
                        4010 or 4011 or 4012 or 4013 => $"Discord closed the connection with code {closed.CloseCode} ({closed.Reason}), this needs a code or Discord.Net update",
                        _ => null
                    };
                }

                // Getting the gateway address fails before connecting when the token is wrong
                if (current is HttpException http && http.HttpCode == HttpStatusCode.Unauthorized) {
                    return "Discord rejected the token (HTTP 401), check discordToken in the settings file";
                }
            }

            return null;
        }

        private static async Task JoinedGuild(SocketGuild guild) {
            LoggingManager.Log.Warn($"Joined guild {guild.Name} ({guild.Id})");

            // Leave if an invalid guild
            if (guild.Id != Constants.XathzDiscordGuild && guild.Id != WubbysFunHouse.ServerId) {
                try {
                    await guild.LeaveAsync(new RequestOptions { AuditLogReason = $"This bot is private. This guild needs to be whitelisted. Contact: {Constants.Creator}" });
                    LoggingManager.Log.Warn($"Left guild {guild.Name} ({guild.Id}), it is not whitelisted");
                } catch (Exception ex) {
                    LoggingManager.Log.Error(ex, $"Failed to leave guild {guild.Name} ({guild.Id})");
                }
            }
        }

        private static Task GuildAvailable(SocketGuild guild) {
            LoggingManager.Log.Info($"Guild {guild.Name} ({guild.Id}) has become available");
            return Task.CompletedTask;
        }

        private static Task GuildMembersDownloaded(SocketGuild guild) {
            LoggingManager.Log.Info($"Downloaded the member list for {guild.Name} ({guild.Id}), {guild.Users.Count:N0} members");
            return Task.CompletedTask;
        }

        private static async Task UserJoined(SocketGuildUser user) {
            if (user.Guild.Id != WubbysFunHouse.ServerId) { return; }

            await SendUserLogAsync($"`[{DateTime.UtcNow.ToString(Constants.DateTimeFormatMedium)}]` :inbox_tray: {user} (`{user.Id}`) has joined.");
        }

        private static async Task UserLeft(SocketGuild guild, SocketUser user) {
            if (guild.Id != WubbysFunHouse.ServerId) { return; }

            await SendUserLogAsync($"`[{DateTime.UtcNow.ToString(Constants.DateTimeFormatMedium)}]` :outbox_tray: {user} (`{user.Id}`) left the server.");
        }

        private static async Task UserBanned(SocketUser user, SocketGuild guild) {
            if (guild.Id != WubbysFunHouse.ServerId) { return; }

            await SendUserLogAsync($"`[{DateTime.UtcNow.ToString(Constants.DateTimeFormatMedium)}]` :no_entry: {user} (`{user.Id}`) was banned.");

            // Look up who banned them, the audit log entry can take a moment to show up
            _ = Task.Run(async () => {
                try {
                    await Task.Delay(4000);

                    if (_DiscordClient.GetChannel(WubbysFunHouse.ModLogsChannelId) is IMessageChannel modLogChannel) {
                        IEnumerable<RestAuditLogEntry> userBans = await guild.GetAuditLogsAsync(10, actionType: ActionType.Ban).FlattenAsync();
                        RestAuditLogEntry bannedEvent = userBans.FirstOrDefault(x => x.Data is BanAuditLogData data && data.Target?.Id == user.Id);

                        if (bannedEvent is null) {
                            await modLogChannel.SendMessageAsync($":no_entry: {user} (`{user.Id}`) was banned, the audit log entry was not found.", allowedMentions: UserMentionsOnly);
                            return;
                        }

                        string bannedBy = bannedEvent.User is IUser moderator ? $"{moderator} (`{moderator.Id}`)" : "an unknown user";

                        if (string.IsNullOrEmpty(bannedEvent.Reason)) {
                            await modLogChannel.SendMessageAsync($":no_entry: {user} (`{user.Id}`) was banned by {bannedBy} with no reason specified.", allowedMentions: UserMentionsOnly);
                        } else {
                            await modLogChannel.SendMessageAsync($":no_entry: {user} (`{user.Id}`) was banned by {bannedBy} with reason: **{bannedEvent.Reason}**", allowedMentions: UserMentionsOnly);
                        }
                    }
                } catch (Exception ex) {
                    LoggingManager.Log.Error(ex);
                }
            });
        }

        private static Task ReactionAdded(Cacheable<IUserMessage, ulong> message, Cacheable<IMessageChannel, ulong> channel, SocketReaction reaction) {

            // Bong trap
            try {
                if (BongTrapMessageId.HasValue && message.Id == BongTrapMessageId && reaction.User.IsSpecified && reaction.User.Value is SocketGuildUser socketUser) {
                    string scrambledName = new string(socketUser.DisplayName.ToCharArray().OrderBy(s => (Random.Shared.Next(2) % 2) == 0).ToArray());
                    _ = socketUser.ModifyAsync(x => x.Nickname = scrambledName);
                }
            } catch (Exception ex) {
                LoggingManager.Log.Error(ex);
            }

            return Task.CompletedTask;
        }

        private static async Task MessageReceived(SocketMessage socketMessage) {

            #region Url in mod mail message

            if (socketMessage.Author.Id == WubbysFunHouse.WubbyMailBotUserId &&
                socketMessage.Channel is SocketTextChannel textChannel &&
                textChannel.CategoryId.HasValue &&
                textChannel.CategoryId.Value == WubbysFunHouse.ModMailCategoryId &&
                socketMessage.Embeds.FirstOrDefault(x => !string.IsNullOrEmpty(x.Description) && x.Description.ContainsUrls()) is Embed urlEmbed) {

                List<string> urls = urlEmbed.Description.GetUrls();
                await socketMessage.Channel.SendMessageAsync($"Url detected in `WubbyMail` message, attempting to embed...{Environment.NewLine}{string.Join(Environment.NewLine, urls)}", allowedMentions: UserMentionsOnly);

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
                    await message.Channel.SendMessageAsync("No commands are available via direct message. Use the slash commands on the server, type `/` to see them.");
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
            // Direct messages and other servers are not logged
            if (!IsWubbysFunHouse(channel)) { return; }

            // Link previews also raise an update, only filter when the text changed or the original is unknown
            bool filter = !originalMessage.HasValue || originalMessage.Value.Content != updatedMessage.Content;

#if DEBUG
            filter &= updatedMessage.Author.Id == Constants.XathzUserId;
#endif

            if (filter && updatedMessage is SocketUserMessage message && message.Source == MessageSource.User &&
                message.Author is SocketGuildUser user && channel is SocketTextChannel textChannel && !WubbysFunHouse.IsDiscordOrTwitchStaff(user)) {
                try {
                    // Otherwise an invite could be edited into a message after it was posted
                    await FilterMessageAsync(textChannel, message, user);
                } catch (Exception ex) {
                    LoggingManager.Log.Error(ex);
                }
            }

            if (originalMessage.HasValue && !string.IsNullOrWhiteSpace(originalMessage.Value.Content) && !string.IsNullOrWhiteSpace(updatedMessage.Content) && !originalMessage.Value.Author.IsBot) {
                IMessage original = originalMessage.Value;

                if (original.Content != updatedMessage.Content) {
                    string header = $"`[{original.CreatedAt.DateTime.ToUniversalTime().ToString(Constants.DateTimeFormatMedium)}]` :pencil: {original.Author} (`{original.Author.Id}`) message edited in **#{channel.Name}**:";
                    string before = original.ResolveTags();
                    string after = updatedMessage.ResolveTags();

                    // Split what is left of the message limit between before and after, with room for the labels
                    int partLength = (DiscordConfig.MaxMessageSize - header.Length - 20) / 2;
                    string fullText = before.Length > partLength || after.Length > partLength ? $"Before:{Environment.NewLine}{original.Content}{Environment.NewLine}{Environment.NewLine}After:{Environment.NewLine}{updatedMessage.Content}" : null;

                    await SendUserLogAsync($"{header}{Environment.NewLine}**B:** {before.Shorten(partLength)}{Environment.NewLine}**A:** {after.Shorten(partLength)}", fullText);
                }
            }
        }

        private static async Task MessageDeleted(Cacheable<IMessage, ulong> message, Cacheable<IMessageChannel, ulong> channel) {

            // Bong trap
            if (message.Id == BongTrapMessageId) {
                BongTrapMessageId = null;
            }

            // Direct messages and other servers are not logged
            if (!IsWubbysFunHouse(channel.Value)) { return; }

            if (message.HasValue && !string.IsNullOrWhiteSpace(message.Value.Content) && !message.Value.Author.IsBot) {
                string header = $"`[{message.Value.CreatedAt.DateTime.ToUniversalTime().ToString(Constants.DateTimeFormatMedium)}]` :wastebasket: {message.Value.Author} (`{message.Value.Author.Id}`) message deleted in **#{channel.Value.Name}**:";
                string content = message.Value.ResolveTags();

                // Room for the header and a new line
                int contentLength = DiscordConfig.MaxMessageSize - header.Length - 2;
                string fullText = content.Length > contentLength ? message.Value.Content : null;

                await SendUserLogAsync($"{header}{Environment.NewLine}{content.Shorten(contentLength)}", fullText);
            }
        }

        /// <summary>
        /// Send a message to the user logs channel.
        /// </summary>
        /// <param name="message">Message to send, must fit in one Discord message.</param>
        /// <param name="fullText">Attached as a text file when set, for messages that had to be shortened.</param>
        private static async Task SendUserLogAsync(string message, string fullText = null) {
            if (!(_DiscordClient.GetChannel(WubbysFunHouse.UserLogsChannelId) is IMessageChannel logChannel)) { return; }

            if (fullText is null) {
                await logChannel.SendMessageAsync(message, allowedMentions: UserMentionsOnly);
            } else {
                using MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(fullText));
                await logChannel.SendFileAsync(stream, "message.txt", message, allowedMentions: UserMentionsOnly);
            }
        }

        /// <summary>
        /// Check if a channel is in Wubby's Fun House.
        /// </summary>
        /// <param name="channel">Channel to check.</param>
        private static bool IsWubbysFunHouse(IChannel channel) => channel is IGuildChannel guildChannel && guildChannel.GuildId == WubbysFunHouse.ServerId;

        /// <summary>
        /// Process additional actions for a message.
        /// </summary>
        /// <param name="channel">Text channel message was sent in.</param>
        /// <param name="message">Message to process.</param>
        /// <param name="user">User who sent the message.</param>
        private static async Task ProcessMessageAsync(SocketTextChannel channel, SocketUserMessage message, SocketGuildUser user) {

            // Staff
            if (WubbysFunHouse.IsDiscordOrTwitchStaff(user)) {
                return;
            }

            try {
                if (await FilterMessageAsync(channel, message, user)) {
                    return;
                }

                // Jingle bells
                if (message.Content.Replace(" ", "").Contains("jingle", StringComparison.OrdinalIgnoreCase)) {
                    Task runner = Task.Run(async () => {
                        await JingleBells(message.Channel.Id).ConfigureAwait(false);
                    });
                }
            } catch (Exception ex) {
                LoggingManager.Log.Error(ex);
            }
        }

        /// <summary>
        /// Remove a message that breaks the server rules. Used for new and edited messages.
        /// </summary>
        /// <param name="channel">Text channel message was sent in.</param>
        /// <param name="message">Message to check.</param>
        /// <param name="user">User who sent the message.</param>
        /// <returns>True if the message was removed.</returns>
        private static async Task<bool> FilterMessageAsync(SocketTextChannel channel, SocketUserMessage message, SocketGuildUser user) {

            // Discord invites, with or without https:// and in either link style
            if (_InviteRegex.IsMatch(message.Content)) {
                await message.DeleteAsync();
                await channel.SendMessageAsync($"{user.Mention} No invites allowed.", allowedMentions: UserMentionsOnly);
                return true;
            }

            // Discord gifts
            if (message.Content.Contains("https://discord.gift/", StringComparison.OrdinalIgnoreCase) && WubbysFunHouse.IsSubscriber(user)) {
                return false;
            }

            // Too many animated emotes in actual fucking spam
            if (channel.Id == WubbysFunHouse.ActualFuckingSpamChannelId) {
                int count = message.Content.CountStringOccurrences("<a:");

                if (count > 10) {
                    await message.DeleteAsync();
                    await channel.SendMessageAsync($"{user.Mention} Only 10 or less animated emotes/emoji per-message. Having many of them breaks the channel for some people.", allowedMentions: UserMentionsOnly);
                    return true;
                }
            }

            return false;
        }

        private static DateTimeOffset _LastJingleBells = DateTimeOffset.UtcNow;
        private static bool _SentJingleBellsCooldownMessage = false;

        /// <summary>
        /// Jingles the bells.
        /// </summary>
        private static async Task JingleBells(ulong channelId) {
            if (_DiscordClient.GetChannel(channelId) is IMessageChannel channel) {
                if (channelId == WubbysFunHouse.ActualFuckingSpamChannelId) { return; }

                if (_LastJingleBells.AddSeconds(240) >= DateTimeOffset.UtcNow) {
                    if (!_SentJingleBellsCooldownMessage) {
                        _SentJingleBellsCooldownMessage = true;

                        string remaining = (_LastJingleBells.AddSeconds(240) - DateTimeOffset.UtcNow).TotalSeconds.Seconds().Humanize(2);
                        await channel.SendMessageAsync($":mrs_claus::skin-tone-5: The elves are sleepy! Check back in {remaining} for some more cheer.");
                    }
                    return;
                }

                _LastJingleBells = DateTimeOffset.UtcNow;
                _SentJingleBellsCooldownMessage = false;

                int msDelay = 2000;
                string dashing = $"{Environment.NewLine}Dashing";

                IUserMessage sentMessage = await channel.SendMessageAsync($":wave::skin-tone-5: _I'm slow so bear with me..._");
                await Task.Delay(2600);

                await sentMessage.ModifyAsync(x => { x.Content = $":snowman2:{dashing}"; });
                await Task.Delay(msDelay);

                await sentMessage.ModifyAsync(x => { x.Content = $":snowman:{dashing} through"; });
                await Task.Delay(msDelay);

                await sentMessage.ModifyAsync(x => { x.Content = $":snowman2:{dashing} through the"; });
                await Task.Delay(msDelay);

                await sentMessage.ModifyAsync(x => { x.Content = $":snowman:{dashing} through the snow"; });
                await Task.Delay(msDelay);

                await sentMessage.ModifyAsync(x => { x.Content = $":snowman2:{dashing} through the snow{Environment.NewLine}In"; });
                await Task.Delay(msDelay);

                await sentMessage.ModifyAsync(x => { x.Content = $":snowman:{dashing} through the snow{Environment.NewLine}In a"; });
                await Task.Delay(msDelay);

                await sentMessage.ModifyAsync(x => { x.Content = $":snowman2:{dashing} through the snow{Environment.NewLine}In a one"; });
                await Task.Delay(msDelay);

                await sentMessage.ModifyAsync(x => { x.Content = $":snowman:{dashing} through the snow{Environment.NewLine}In a one-horse"; });
                await Task.Delay(msDelay);

                await sentMessage.ModifyAsync(x => { x.Content = $":snowman2:{dashing} through the snow{Environment.NewLine}In a one-horse open"; });
                await Task.Delay(msDelay);

                await sentMessage.ModifyAsync(x => { x.Content = $":snowman:{dashing} through the snow{Environment.NewLine}In a one-horse open sleigh"; });
                await Task.Delay(msDelay);

                await sentMessage.ModifyAsync(x => { x.Content = $":cloud_snow:{dashing} through the snow{Environment.NewLine}In a one-horse open sleigh{Environment.NewLine}O'er"; });
                await Task.Delay(msDelay);

                await sentMessage.ModifyAsync(x => { x.Content = $":snowflake:{dashing} through the snow{Environment.NewLine}In a one-horse open sleigh{Environment.NewLine}O'er the"; });
                await Task.Delay(msDelay);

                await sentMessage.ModifyAsync(x => { x.Content = $":cloud_snow:{dashing} through the snow{Environment.NewLine}In a one-horse open sleigh{Environment.NewLine}O'er the fields"; });
                await Task.Delay(msDelay);

                await sentMessage.ModifyAsync(x => { x.Content = $":snowflake:{dashing} through the snow{Environment.NewLine}In a one-horse open sleigh{Environment.NewLine}O'er the fields we"; });
                await Task.Delay(msDelay);

                await sentMessage.ModifyAsync(x => { x.Content = $":cloud_snow:{dashing} through the snow{Environment.NewLine}In a one-horse open sleigh{Environment.NewLine}O'er the fields we go"; });
                await Task.Delay(msDelay);

                await sentMessage.ModifyAsync(x => { x.Content = $":laughing:{dashing} through the snow{Environment.NewLine}In a one-horse open sleigh{Environment.NewLine}O'er the fields we go{Environment.NewLine}Laughing"; });
                await Task.Delay(msDelay);

                await sentMessage.ModifyAsync(x => { x.Content = $":rofl:{dashing} through the snow{Environment.NewLine}In a one-horse open sleigh{Environment.NewLine}O'er the fields we go{Environment.NewLine}Laughing all"; });
                await Task.Delay(msDelay);

                await sentMessage.ModifyAsync(x => { x.Content = $":laughing:{dashing} through the snow{Environment.NewLine}In a one-horse open sleigh{Environment.NewLine}O'er the fields we go{Environment.NewLine}Laughing all the"; });
                await Task.Delay(msDelay);

                await sentMessage.ModifyAsync(x => { x.Content = $":rofl:{dashing} through the snow{Environment.NewLine}In a one-horse open sleigh{Environment.NewLine}O'er the fields we go{Environment.NewLine}Laughing all the way"; });
                await Task.Delay(4200);

                await sentMessage.ModifyAsync(x => { x.Content = $":christmas_tree: :candle: **Happy holidays** :menorah: :star2:"; });
            }
        }

    }

}
