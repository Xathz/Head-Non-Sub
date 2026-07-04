using System;
using System.Threading.Tasks;
using HeadNonSub.Database;
using HeadNonSub.Settings;
using Humanizer;
using Microsoft.Extensions.Options;
using TwitchLib.Api;
using TwitchLib.Api.Core;
using TwitchLib.Api.Helix.Models.Users.GetUsers;
using TwitchLib.Api.Services;
using TwitchLib.Api.Services.Events.LiveStreamMonitor;
using TwitchLib.Api.ThirdParty.AuthorizationFlow;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Events;
using Client = TwitchLib.Client;

namespace HeadNonSub.Clients.Twitch {

    public static class TwitchClient {

        private static ApiSettings _ApiSettings;
        private static ConnectionCredentials _ConnectionCredentials;

        private static TwitchAPI _TwitchApi;
        private static Client.TwitchClient _TwitchClient;

        private static LiveStreamMonitorService _StreamMonitor;

        /// <summary>
        /// Optional IOptions configuration provider (for dependency injection).
        /// </summary>
        private static IOptions<Configuration> _ConfigurationOptions;

        public static bool IsLive { private set; get; } = false;

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
            return GetConfiguration();
        }

        public static async Task ConnectApiAsync() {
            try {
                LoggingManager.Log.Info("Connecting");

                _ApiSettings = new ApiSettings {
                    ClientId = GetConfiguration().TwitchClientId,
                    AccessToken = GetConfiguration().TwitchToken
                };

                _TwitchApi = new TwitchAPI(settings: _ApiSettings);

                LoggingManager.Log.Info("Connected");

                bool wasRefreshed = false;
                RefreshTokenResponse refresh = _TwitchApi.ThirdParty.AuthorizationFlow.RefreshToken(GetConfiguration().TwitchRefresh);

                if (!string.IsNullOrEmpty(refresh.Refresh)) {
                    GetConfiguration().TwitchRefresh = refresh.Refresh;
                    wasRefreshed = true;
                }

                if (!string.IsNullOrEmpty(refresh.Token)) {
                    GetConfiguration().TwitchToken = refresh.Token;
                    wasRefreshed = true;
                }

                if (wasRefreshed) { SettingsManager.Save(); }

                //_TwitchApi.ThirdParty.AuthorizationFlow.OnUserAuthorizationDetected += (s, a) => {
                //    Console.WriteLine($"      id: {a.Id}");
                //    Console.WriteLine($"username: {a.Username}");
                //    Console.WriteLine($"   token: {a.Token}");
                //    Console.WriteLine($" refresh: {a.Refresh}");
                //    Console.WriteLine($"  scopes: {a.Scopes}");
                //};

#if DEBUG
                return;
#endif

                // Get user id's for streams that do not have it set
                if (await CompleteTwitchStreamSettingsAsync()) {
                    StartMonitor();
                } else {
                    LoggingManager.Log.Error("Failed to complete Twitch stream settings, skipping stream monitoring.");
                }

            } catch (Exception ex) {
                LoggingManager.Log.Error(ex);
            }
        }

        public static void ConnectClient() {
            try {
                LoggingManager.Log.Info("Connecting");

                _ConnectionCredentials = new ConnectionCredentials(GetConfiguration().TwitchUsername, GetConfiguration().TwitchToken);

                _TwitchClient = new Client.TwitchClient {
                    AutoReListenOnException = true
                };

                _TwitchClient.Initialize(_ConnectionCredentials, GetConfiguration().TwitchStream.Username);

                _TwitchClient.Connect();

#if DEBUG
                _TwitchClient.OnLog += (s, e) => { LoggingManager.Log.Debug(e.Data); };
#endif

                _TwitchClient.OnConnected += OnConnected;
                _TwitchClient.OnDisconnected += OnDisconnected;
                _TwitchClient.OnReconnected += OnReconnected;

                _TwitchClient.OnJoinedChannel += OnJoinedChannel;

                LoggingManager.Log.Info("Connected");

            } catch (Exception ex) {
                LoggingManager.Log.Error(ex);
            }
        }

        private static async Task<bool> CompleteTwitchStreamSettingsAsync() {
            if (string.IsNullOrWhiteSpace(GetConfiguration().TwitchStream.UserId) & string.IsNullOrWhiteSpace(GetConfiguration().TwitchStream.DisplayName)) {
                LoggingManager.Log.Error("Both the Twitch stream user id and display name are empty.");
                return false;
            }

            if (GetConfiguration().TwitchStream.DiscordChannel == 0) {
                LoggingManager.Log.Error("The Discord channel is not set.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(GetConfiguration().TwitchStream.UserId)) {
                try {
                    GetUsersResponse result = await _TwitchApi.Helix.Users.GetUsersAsync(logins: GetConfiguration().TwitchStream.UsernameAsList);

                    if (result.Users.Length > 0) {
                        GetConfiguration().TwitchStream.UserId = result.Users[0].Id;
                    }

                    LoggingManager.Log.Info($"Retrieved user id for username: {GetConfiguration().TwitchStream.Username}");
                    SettingsManager.Save();

                } catch (Exception ex) {
                    LoggingManager.Log.Error(ex);
                }
            } else if (string.IsNullOrWhiteSpace(GetConfiguration().TwitchStream.DisplayName)) {
                try {
                    GetUsersResponse result = await _TwitchApi.Helix.Users.GetUsersAsync(ids: GetConfiguration().TwitchStream.UserIdAsList);

                    if (result.Users.Length > 0) {
                        GetConfiguration().TwitchStream.DisplayName = result.Users[0].DisplayName;
                    }

                    LoggingManager.Log.Info($"Retrieved display name for user id: {GetConfiguration().TwitchStream.UserId}");
                    SettingsManager.Save();

                } catch (Exception ex) {
                    LoggingManager.Log.Error(ex);
                }
            }

            return true;
        }

        private static void StartMonitor() {
            try {
                _StreamMonitor = new LiveStreamMonitorService(_TwitchApi, 30);
                _StreamMonitor.SetChannelsByName(GetConfiguration().TwitchStream.UsernameAsList);

                _StreamMonitor.Start();

                _StreamMonitor.OnStreamOnline += OnStreamOnline;
                _StreamMonitor.OnStreamOffline += OnStreamOffline;

                LoggingManager.Log.Info($"Stream monitoring is running for: {GetConfiguration().TwitchStream.DisplayName}");
            } catch (Exception ex) {
                LoggingManager.Log.Error(ex);
            }
        }

        private static async void OnStreamOnline(object sender, OnStreamOnlineArgs streamOnline) {
            HostingMonitor.StopMonitor();

            if (DatabaseManager.ActiveStreams.Insert(GetConfiguration().TwitchStream.Username)) {
                await Task.Delay(8000);

                IsLive = true;

                _ = Discord.DiscordClient.SetStatus($"Watching {GetConfiguration().TwitchStream.DisplayName}!", GetConfiguration().TwitchStream.Url);
                _ = Discord.DiscordClient.TwitchChannelChange(GetConfiguration().TwitchStream.DiscordChannel, GetConfiguration().TwitchStream.Url, streamOnline.Stream.ThumbnailUrl, $"{GetConfiguration().TwitchStream.DisplayName} is live!", streamOnline.Stream.Title, true, true);

                LoggingManager.Log.Info($"{GetConfiguration().TwitchStream.DisplayName} just went live");
            } else {
                _ = Discord.DiscordClient.SetStatus($"Watching {GetConfiguration().TwitchStream.DisplayName}!", GetConfiguration().TwitchStream.Url);

                LoggingManager.Log.Info($"{GetConfiguration().TwitchStream.DisplayName} is still live");
            }
        }

        private static void OnStreamOffline(object sender, OnStreamOfflineArgs streamOffline) {
            //HostingMonitor.StartMonitor();

            IsLive = false;

            DateTime? startedAt = DatabaseManager.ActiveStreams.Delete(GetConfiguration().TwitchStream.Username);
            _ = Discord.DiscordClient.SetStatus();

            string duration = startedAt.HasValue ? $"They were live for {(DateTime.UtcNow - startedAt.Value).TotalMilliseconds.Milliseconds().Humanize(3)}{Environment.NewLine}" : "";
            _ = Discord.DiscordClient.TwitchChannelChange(GetConfiguration().TwitchStream.DiscordChannel, GetConfiguration().TwitchStream.Url, null, $"{GetConfiguration().TwitchStream.DisplayName} is offline", $"{duration}Thanks for watching!");

            LoggingManager.Log.Info($"{GetConfiguration().TwitchStream.DisplayName} is offline");
        }

        private static void OnConnected(object sender, Client.Events.OnConnectedArgs connected) => LoggingManager.Log.Info("Connected to Twitch");

        private static void OnDisconnected(object sender, OnDisconnectedEventArgs disconnected) => LoggingManager.Log.Info("Disconnected from Twitch");

        private static void OnReconnected(object sender, OnReconnectedEventArgs reconnected) => LoggingManager.Log.Info("Reconnected to Twitch");

        private static void OnJoinedChannel(object sender, Client.Events.OnJoinedChannelArgs joined) => LoggingManager.Log.Info($"Joined Twitch channel: {joined.Channel}");

    }

}
