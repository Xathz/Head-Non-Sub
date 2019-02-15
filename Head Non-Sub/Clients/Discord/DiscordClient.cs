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

        public static async Task ConnectAsync() {
            _DiscordConfig = new DiscordSocketConfig {
                MessageCacheSize = 200,
                DefaultRetryMode = RetryMode.RetryRatelimit
            };

            _DiscordClient = new DiscordSocketClient(_DiscordConfig);

            _Services = new ServiceCollection().AddSingleton(_DiscordClient)
                .AddSingleton<CommandService>()
                .AddSingleton<DiscordClientCommandService>()
                .BuildServiceProvider();

            _DiscordClient.Log += Log;
            _DiscordClient.Connected += Connected;
            _DiscordClient.GuildAvailable += GuildAvailable;
            _DiscordClient.GuildMembersDownloaded += GuildMembersDownloaded;
            _DiscordClient.MessageReceived += MessageReceived;

            await _DiscordClient.LoginAsync(TokenType.Bot, SettingsManager.Configuration.DiscordToken);
            await _DiscordClient.StartAsync();

            _Services.GetRequiredService<CommandService>().Log += Log;
            await _Services.GetRequiredService<DiscordClientCommandService>().InitializeAsync();
        }

        public static async Task StopAsync() => await _DiscordClient.StopAsync();

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
                arg.DownloadUsersAsync();
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
