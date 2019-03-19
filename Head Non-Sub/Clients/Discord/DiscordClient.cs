using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using HeadNonSub.Settings;
using Microsoft.Extensions.DependencyInjection;

namespace HeadNonSub.Clients.Discord {

    public static class DiscordClient {

        private static DiscordSocketConfig _DiscordConfig;
        private static DiscordSocketClient _DiscordClient;

        private static IServiceProvider _Services_Mention;
        private static IServiceProvider _Services_Exclamation;

        public static async Task ConnectAsync() {
            _DiscordConfig = new DiscordSocketConfig {
                DefaultRetryMode = RetryMode.RetryRatelimit,
                MessageCacheSize = 1000,
                LogLevel = LogSeverity.Info
            };

            _DiscordClient = new DiscordSocketClient(_DiscordConfig);

            _Services_Mention = new ServiceCollection().AddSingleton(_DiscordClient)
                .AddSingleton(new CommandService(new CommandServiceConfig() {
                    DefaultRunMode = RunMode.Async,
                    LogLevel = LogSeverity.Info
                }))
                .AddSingleton<CommandService_Mention>()
                .BuildServiceProvider();

            _Services_Exclamation = new ServiceCollection().AddSingleton(_DiscordClient)
                .AddSingleton(new CommandService(new CommandServiceConfig() {
                    DefaultRunMode = RunMode.Async,
                    LogLevel = LogSeverity.Info
                }))
                .AddSingleton<CommandService_Exclamation>()
                .BuildServiceProvider();

            _DiscordClient.Log += Log;
            _DiscordClient.Connected += Connected;
            _DiscordClient.GuildAvailable += GuildAvailable;
            _DiscordClient.GuildMembersDownloaded += GuildMembersDownloaded;
            _DiscordClient.MessageReceived += MessageReceived;

            _Services_Mention.GetRequiredService<CommandService>().Log += Log;
            _Services_Exclamation.GetRequiredService<CommandService>().Log += Log;

            await _Services_Mention.GetRequiredService<CommandService_Mention>().InitializeAsync();
            await _Services_Exclamation.GetRequiredService<CommandService_Exclamation>().InitializeAsync();

            await _DiscordClient.LoginAsync(TokenType.Bot, SettingsManager.Configuration.DiscordToken);
            await _DiscordClient.StartAsync();
        }

        public static async Task StopAsync() => await _DiscordClient.StopAsync();

        public static void FailFast() {
            _ = _DiscordClient.LogoutAsync();
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
                        builder.ImageUrl = imageUrl.Replace("{width}", "1920").Replace("{height}", "1080");
                    }

                    builder.Author = new EmbedAuthorBuilder {
                        Name = displayName,
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

        private static Task Log(LogMessage logMessage) {
            switch (logMessage.Severity) {
                case LogSeverity.Debug:
                    LoggingManager.Log.Debug(logMessage.Message);
                    return Task.CompletedTask;

                case LogSeverity.Verbose:
                    LoggingManager.Log.Trace(logMessage.Message);
                    return Task.CompletedTask;

                case LogSeverity.Info:
                    LoggingManager.Log.Info(logMessage.Message);
                    return Task.CompletedTask;

                case LogSeverity.Warning:
                    LoggingManager.Log.Warn(logMessage.Message);
                    return Task.CompletedTask;

                case LogSeverity.Error:
                    LoggingManager.Log.Error(logMessage.Exception, logMessage.Message);
                    return Task.CompletedTask;

                case LogSeverity.Critical:
                    LoggingManager.Log.Fatal(logMessage.Exception, logMessage.Message);
                    return Task.CompletedTask;

                default:
                    LoggingManager.Log.Info($"UnknownSeverity: {logMessage.Message}");
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

        private static async Task GuildAvailable(SocketGuild guild) {
            LoggingManager.Log.Info($"Guild {guild.Name} ({guild.Id}) has become available");

            try {
                await guild.DownloadUsersAsync();
            } catch (Exception ex) {
                LoggingManager.Log.Error(ex);
            }
        }

        private static Task GuildMembersDownloaded(SocketGuild guild) {
            LoggingManager.Log.Info($"Full memberlist was downloaded for {guild.Name} ({guild.Id})");
            return Task.CompletedTask;
        }

        private static async Task MessageReceived(SocketMessage socketMessage) {
            if (!(socketMessage is SocketUserMessage message)) { return; }
            if (!(socketMessage.Channel is IPrivateChannel channel)) { return; }
            if (socketMessage.Source != MessageSource.User) { return; }

            // Accepting commands to HeadNonSub.Clients.Discord.Commands.Exclamation.ImageTemplates
            //await message.Channel.SendMessageAsync($"No commands are available via direct message. Please @ the bot (`@{_DiscordClient.CurrentUser.Username}`) on the server.");
        }

    }

}
