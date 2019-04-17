using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using HeadNonSub.Extensions;
using HeadNonSub.Settings;
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

        private static IServiceProvider _DynamicProvider;
        private static readonly CommandService _DynamicService = new CommandService(_ServiceConfig);

        private static readonly List<string> _ValidPolls = new List<string>() { "poll:", "!strawpoll", "!strawpollresults", "!strawpollr", "!trashpoll" };

        public static async Task ConnectAsync() {
            _DiscordConfig = new DiscordSocketConfig {
                DefaultRetryMode = RetryMode.RetryRatelimit,
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

            _DynamicProvider = new ServiceCollection().AddSingleton(_DiscordClient)
                .AddSingleton(_DynamicService)
                .AddSingleton<DynamicCommands>()
                .BuildServiceProvider();

            _DiscordClient.Log += Log;
            _DiscordClient.Connected += Connected;

            _DiscordClient.JoinedGuild += JoinedGuild;
            _DiscordClient.GuildAvailable += GuildAvailable;
            _DiscordClient.GuildMembersDownloaded += GuildMembersDownloaded;

            _DiscordClient.MessageReceived += MessageReceived;

            _MentionProvider.GetRequiredService<CommandService>().Log += Log;
            _ExclamationProvider.GetRequiredService<CommandService>().Log += Log;
            _DynamicProvider.GetRequiredService<CommandService>().Log += Log;

            await _MentionProvider.GetRequiredService<MentionCommands>().InitializeAsync();
            MentionCommandList = _MentionProvider.GetRequiredService<MentionCommands>().CommandList;

            await _ExclamationProvider.GetRequiredService<ExclamationCommands>().InitializeAsync();
            ExclamationCommandList = _ExclamationProvider.GetRequiredService<ExclamationCommands>().CommandList;

            await _DynamicProvider.GetRequiredService<DynamicCommands>().InitializeAsync();

            await _DiscordClient.LoginAsync(TokenType.Bot, SettingsManager.Configuration.DiscordToken);
            await _DiscordClient.StartAsync();
        }

        public static ReadOnlyCollection<CommandInfo> MentionCommandList { get; private set; }

        public static ReadOnlyCollection<CommandInfo> ExclamationCommandList { get; private set; }

        public static async Task StopAsync() => await _DiscordClient.StopAsync();

        public static async void FailFast() {
            try {
                File.WriteAllText(Constants.FailFastFile, "This file was generated becuase the fail fast command was executed." +
                     " The watchdog script (Watchdog.ps1) will not start/restart the bot if this file is here.");

            } catch (Exception ex) {
                LoggingManager.Log.Error(ex);
            }

            await _DiscordClient.LogoutAsync();
            Environment.Exit(13);
        }

        public static async Task<ulong?> SendMessageToChannelAsync(ulong channelId, string message) {
            try {
                if (_DiscordClient.GetChannel(channelId) is IMessageChannel channel) {
                    return (await channel.SendMessageAsync(message)).Id;
                }

                return null;
            } catch (Exception ex) {
                LoggingManager.Log.Error(ex);
                return null;
            }
        }

        public static async Task TwitchChannelChange(ulong discordChannel, string streamUrl, string displayName, string imageUrl, string title, string description, bool everyone = false) {
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
                        IconUrl = Constants.TwitchLogoTransparent,
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
            if (guild.Id != Constants.XathzDiscordGuild || guild.Id != Constants.WubbyDiscordGuild) {
                guild.LeaveAsync(new RequestOptions { AuditLogReason = $"This bot is private. This guild needs to be whitelisted. Contact: {Constants.Creator}" });
            }

            return Task.CompletedTask;
        }

        private static Task GuildAvailable(SocketGuild guild) {
            LoggingManager.Log.Info($"Guild {guild.Name} ({guild.Id}) has become available");
            return Task.CompletedTask;
        }

        private static Task GuildMembersDownloaded(SocketGuild guild) {
            LoggingManager.Log.Info($"Full memberlist was downloaded for {guild.Name} ({guild.Id})");
            return Task.CompletedTask;
        }

        private static async Task MessageReceived(SocketMessage socketMessage) {
            if (!(socketMessage is SocketUserMessage message)) { return; }
            if (socketMessage.Source != MessageSource.User) { return; }

            if (socketMessage.Channel is IPrivateChannel channel) {
                await message.Channel.SendMessageAsync($"No commands are available via direct message. Please @ the bot (`@{_DiscordClient.CurrentUser.Username}`) on the server.");
                return;
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

        private static async Task ProcessMessageAsync(SocketUserMessage message, SocketGuildUser user) {

            // If discord staff exit
            if (user.Roles.Any(x => WubbysFunHouse.DiscordStaffRoles.Contains(x.Id))) {
                return;
            }

            string betterUserFormat = $"{(string.IsNullOrWhiteSpace(user.Nickname) ? user.Username : user.Nickname)} `{user.ToString()}`";

            try {
                // If not rank 10 or higher
                if (!user.Roles.Any(x => x.Id == WubbysFunHouse.NakedCowboyRoleId)) {
                    if (message.Channel.Id == WubbysFunHouse.MainChannelId) {
                        if (message.Content.ContainsUrls()) {
                            if (_DiscordClient.GetChannel(WubbysFunHouse.LinksChannelId) is IMessageChannel channel) {
                                LoggingManager.Log.Info($"Link in #{message.Channel.Name} by {message.Author.ToString()} ({message.Author.Id})");

                                await message.DeleteAsync();
                                await channel.SendMessageAsync($"● Posted by {betterUserFormat} in <#{WubbysFunHouse.MainChannelId}>{Environment.NewLine}{message.Content}");
                                await message.Channel.SendMessageAsync($"{user.Mention} You need to be <@&{WubbysFunHouse.NakedCowboyRoleId}> or higher to post links here. Link was moved to <#{WubbysFunHouse.LinksChannelId}>.");
                            }
                        } else if (message.Attachments.Count > 0) {
                            if (_DiscordClient.GetChannel(WubbysFunHouse.ActualFuckingSpamChannelId) is IMessageChannel channel) {
                                foreach (Attachment attachment in message.Attachments) {
                                    using (WebClient webClient = new WebClient())
                                    using (MemoryStream stream = new MemoryStream(webClient.DownloadData(attachment.Url))) {
                                        LoggingManager.Log.Info($"Attachment in #{message.Channel.Name} by {message.Author.ToString()} ({message.Author.Id}); {attachment.Filename}; api: {attachment.Size.Bytes().Humanize("#.##")}; downloaded: {stream.Length.Bytes().Humanize("#.##")}");

                                        if (stream.Length > 8388119) {
                                            await channel.SendMessageAsync($"An attachment was uploaded by {betterUserFormat} in <#{WubbysFunHouse.MainChannelId}> and can not be re-uploaded, the attachment is too large for a bot to upload (`{attachment.Size.Bytes().Humanize("#.##")} / {stream.Length.Bytes().Humanize("#.##")}`). They probably have Nitro.");
                                        } else {
                                            stream.Seek(0, SeekOrigin.Begin);
                                            await channel.SendFileAsync(stream, attachment.Filename, $"● Uploaded by {betterUserFormat} in <#{WubbysFunHouse.MainChannelId}>{Environment.NewLine}{message.Content}");
                                        }
                                    }
                                }

                                await message.DeleteAsync();
                                await message.Channel.SendMessageAsync($"{user.Mention} You need to be <@&{WubbysFunHouse.NakedCowboyRoleId}> or higher to upload files here. Attachment was moved to <#{WubbysFunHouse.ActualFuckingSpamChannelId}>.");
                            }
                        }
                    }
                }

                if (message.Channel.Id == WubbysFunHouse.MarketResearchChannelId) {
                    string channelName = _DiscordClient.GetGuild(WubbysFunHouse.ServerId).GetChannel(WubbysFunHouse.MarketResearchChannelId).Name;

                    if (!_ValidPolls.Any(x => message.Content.Contains(x, StringComparison.OrdinalIgnoreCase))) {
                        LoggingManager.Log.Info($"Invalid poll in #{channelName} by {message.Author.ToString()} ({message.Author.Id})");

                        await message.DeleteAsync();
                        await message.Author.SendMessageAsync($"You can only post polls in #{channelName} on {_DiscordClient.GetGuild(WubbysFunHouse.ServerId).Name}.");
                    }
                }
            } catch (Exception ex) {
                LoggingManager.Log.Error(ex);
            }
        }

    }

}
