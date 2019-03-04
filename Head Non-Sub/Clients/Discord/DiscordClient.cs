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

        private static IServiceProvider _Services;
        private static IServiceProvider _Services_Exclamation;
        private static IServiceProvider _Services_Oof;

        public static async Task ConnectAsync() {
            _DiscordConfig = new DiscordSocketConfig {
                DefaultRetryMode = RetryMode.RetryRatelimit,
                MessageCacheSize = 1000,
                LogLevel = LogSeverity.Info
            };

            _DiscordClient = new DiscordSocketClient(_DiscordConfig);

            _Services = new ServiceCollection().AddSingleton(_DiscordClient)
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

            _Services_Oof = new ServiceCollection().AddSingleton(_DiscordClient)
                .AddSingleton(new CommandService(new CommandServiceConfig() {
                    DefaultRunMode = RunMode.Async,
                    LogLevel = LogSeverity.Info
                }))
                .AddSingleton<CommandService_Oof>()
                .BuildServiceProvider();

            _DiscordClient.Log += Log;
            _DiscordClient.Connected += Connected;
            _DiscordClient.GuildAvailable += GuildAvailable;
            _DiscordClient.GuildMembersDownloaded += GuildMembersDownloaded;
            _DiscordClient.MessageReceived += MessageReceived;

            _Services.GetRequiredService<CommandService>().Log += Log;
            _Services_Exclamation.GetRequiredService<CommandService>().Log += Log;
            _Services_Oof.GetRequiredService<CommandService>().Log += Log;

            await _Services.GetRequiredService<CommandService_Mention>().InitializeAsync();
            await _Services_Exclamation.GetRequiredService<CommandService_Exclamation>().InitializeAsync();
            await _Services_Oof.GetRequiredService<CommandService_Oof>().InitializeAsync();

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

        public static async Task TwitchChannelChange(string channelName, string imageUrl, string title, string description) {
            try {
                // '#main (403341336129830918)' on 'Wubby's Fun House (328300333010911242)'
                if (_DiscordClient.GetChannel(403341336129830918) is IMessageChannel channel) {
                    string url = imageUrl.Replace("{width}", "1920").Replace("{height}", "1080");

                    EmbedBuilder builder = new EmbedBuilder() {
                        Color = new Color(Constants.GeneralColor.R, Constants.GeneralColor.G, Constants.GeneralColor.B),
                        ImageUrl = url,
                        Url = $"https://twitch.tv/{channelName}",
                        Title = title,
                        Description = description
                    };

                    builder.Author = new EmbedAuthorBuilder {
                        Name = channelName,
                        Url = $"https://twitch.tv/{channelName}"
                    };

                    await channel.SendMessageAsync(embed: builder.Build());
                }
            } catch (Exception ex) {
                LoggingManager.Log.Error(ex);
            }
        }

        private static Task Log(LogMessage e) {
            switch (e.Severity) {
                case LogSeverity.Debug:
                    LoggingManager.Log.Debug(e.Message);
                    return Task.CompletedTask;

                case LogSeverity.Verbose:
                    LoggingManager.Log.Trace(e.Message);
                    return Task.CompletedTask;

                case LogSeverity.Info:
                    LoggingManager.Log.Info(e.Message);
                    return Task.CompletedTask;

                case LogSeverity.Warning:
                    LoggingManager.Log.Warn(e.Message);
                    return Task.CompletedTask;

                case LogSeverity.Error:
                    LoggingManager.Log.Error(e.Exception, e.Message);
                    return Task.CompletedTask;

                case LogSeverity.Critical:
                    LoggingManager.Log.Fatal(e.Exception, e.Message);
                    return Task.CompletedTask;

                default:
                    LoggingManager.Log.Info($"UnknownSeverity: {e.Message}");
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

        private static async Task GuildAvailable(SocketGuild arg) {
            LoggingManager.Log.Info($"Guild {arg.Name} ({arg.Id}) has become available");

            try {
                _ = arg.DownloadUsersAsync();
            } catch (Exception ex) {
                LoggingManager.Log.Error(ex);
            }
        }

        private static async Task GuildMembersDownloaded(SocketGuild arg) {
            LoggingManager.Log.Info($"Full memberlist was downloaded for {arg.Name} ({arg.Id})");

        }

        private static async Task MessageReceived(SocketMessage e) {
            if (!(e is SocketUserMessage message)) { return; }
            if (!(e.Channel is IPrivateChannel channel)) { return; }
            if (e.Source != MessageSource.User) { return; }

            await message.Channel.SendMessageAsync($"No commands are available via direct message. Please @ the bot (`@{_DiscordClient.CurrentUser.Username}`) on the server.");
        }

    }

}
