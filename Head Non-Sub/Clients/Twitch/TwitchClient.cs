using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HeadNonSub.Database;
using HeadNonSub.Exceptions;
using HeadNonSub.Settings;
using Humanizer;
using TwitchLib.Api;
using TwitchLib.Api.Core;
using TwitchLib.Api.Helix.Models.Clips.GetClip;
using TwitchLib.Api.Helix.Models.Users;
using TwitchLib.Api.Services;
using TwitchLib.Api.Services.Events.LiveStreamMonitor;
using TwitchLib.Api.ThirdParty.AuthorizationFlow;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Events;
using Client = TwitchLib.Client;
using TwitchEntities = HeadNonSub.Entities.Twitch;

namespace HeadNonSub.Clients.Twitch {

    public static class TwitchClient {

        private static ApiSettings _ApiSettings;
        private static ConnectionCredentials _ConnectionCredentials;

        private static TwitchAPI _TwitchApi;
        private static Client.TwitchClient _TwitchClient;

        private static LiveStreamMonitorService _StreamMonitor;

        public static bool IsLive { private set; get; } = false;

        public static async Task ConnectApiAsync() {
            try {
                LoggingManager.Log.Info("Connecting");

                _ApiSettings = new ApiSettings {
                    ClientId = SettingsManager.Configuration.TwitchClientId,
                    AccessToken = SettingsManager.Configuration.TwitchToken
                };

                _TwitchApi = new TwitchAPI(settings: _ApiSettings);

                LoggingManager.Log.Info("Connected");

                RefreshTokenResponse refresh = _TwitchApi.ThirdParty.AuthorizationFlow.RefreshToken(SettingsManager.Configuration.TwitchRefresh);
                SettingsManager.Configuration.TwitchRefresh = refresh.Refresh;
                SettingsManager.Configuration.TwitchToken = refresh.Token;
                SettingsManager.Save();

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

                _ConnectionCredentials = new ConnectionCredentials(SettingsManager.Configuration.TwitchUsername, SettingsManager.Configuration.TwitchToken);

                _TwitchClient = new Client.TwitchClient {
                    AutoReListenOnException = true
                };

                _TwitchClient.Initialize(_ConnectionCredentials, SettingsManager.Configuration.TwitchStream.Username);

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

        /// <summary>
        /// Get 100 most recent clips from a stream.
        /// </summary>
        /// <param name="userId">User id of the stream.</param>
        /// <param name="displayName">Display name of the stream.</param>
        public static async Task<List<TwitchEntities.Clip>> GetClipsAsync(string userId, string displayName) {
            if (!string.IsNullOrEmpty(userId)) {
                List<TwitchEntities.Clip> returnClips = new List<TwitchEntities.Clip>();

                GetClipResponse result = await _TwitchApi.Helix.Clips.GetClipAsync(broadcasterId: userId, first: 100);
                foreach (Clip clip in result.Clips) {
                    returnClips.Add(new TwitchEntities.Clip(DateTime.Parse(clip.CreatedAt), clip.Title, clip.ViewCount, clip.Url));
                }

                LoggingManager.Log.Info($"Retrieved {returnClips.Count} for {displayName} ({userId})");

                return returnClips;
            } else {
                throw new UnsupportedTwitchChannelException($"{displayName} is not a supported Twitch channel at this time.");
            }
        }

        private static async Task<bool> CompleteTwitchStreamSettingsAsync() {
            if (string.IsNullOrWhiteSpace(SettingsManager.Configuration.TwitchStream.UserId) & string.IsNullOrWhiteSpace(SettingsManager.Configuration.TwitchStream.DisplayName)) {
                LoggingManager.Log.Error("Both the Twitch stream user id and display name are empty.");
                return false;
            }

            if (SettingsManager.Configuration.TwitchStream.DiscordChannel == 0) {
                LoggingManager.Log.Error("The Discord channel is not set.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(SettingsManager.Configuration.TwitchStream.UserId)) {
                try {
                    GetUsersResponse result = await _TwitchApi.Helix.Users.GetUsersAsync(logins: SettingsManager.Configuration.TwitchStream.UsernameAsList);

                    if (result.Users.Length > 0) {
                        SettingsManager.Configuration.TwitchStream.UserId = result.Users[0].Id;
                    }

                    LoggingManager.Log.Info($"Retrieved user id for username: {SettingsManager.Configuration.TwitchStream.Username}");
                    SettingsManager.Save();

                } catch (Exception ex) {
                    LoggingManager.Log.Error(ex);
                }
            } else if (string.IsNullOrWhiteSpace(SettingsManager.Configuration.TwitchStream.DisplayName)) {
                try {
                    GetUsersResponse result = await _TwitchApi.Helix.Users.GetUsersAsync(ids: SettingsManager.Configuration.TwitchStream.UserIdAsList);

                    if (result.Users.Length > 0) {
                        SettingsManager.Configuration.TwitchStream.DisplayName = result.Users[0].DisplayName;
                    }

                    LoggingManager.Log.Info($"Retrieved display name for user id: {SettingsManager.Configuration.TwitchStream.UserId}");
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
                _StreamMonitor.SetChannelsByName(SettingsManager.Configuration.TwitchStream.UsernameAsList);

                _StreamMonitor.Start();

                _StreamMonitor.OnStreamOnline += OnStreamOnline;
                _StreamMonitor.OnStreamOffline += OnStreamOffline;

                LoggingManager.Log.Info($"Stream monitoring is running for: {SettingsManager.Configuration.TwitchStream.DisplayName}");
            } catch (Exception ex) {
                LoggingManager.Log.Error(ex);
            }
        }

        private static void OnStreamOnline(object sender, OnStreamOnlineArgs streamOnline) {
            HostingMonitor.StopMonitor();

            if (DatabaseManager.ActiveStreams.Insert(SettingsManager.Configuration.TwitchStream.Username)) {
                IsLive = true;

                _ = Discord.DiscordClient.SetStatus($"Watching {SettingsManager.Configuration.TwitchStream.DisplayName}!", SettingsManager.Configuration.TwitchStream.Url);
                _ = Discord.DiscordClient.TwitchChannelChange(SettingsManager.Configuration.TwitchStream.DiscordChannel, SettingsManager.Configuration.TwitchStream.Url, streamOnline.Stream.ThumbnailUrl, $"{SettingsManager.Configuration.TwitchStream.DisplayName} is live!", streamOnline.Stream.Title, true);

                LoggingManager.Log.Info($"{SettingsManager.Configuration.TwitchStream.DisplayName} just went live");
            } else {
                _ = Discord.DiscordClient.SetStatus($"Watching {SettingsManager.Configuration.TwitchStream.DisplayName}!", SettingsManager.Configuration.TwitchStream.Url);

                LoggingManager.Log.Info($"{SettingsManager.Configuration.TwitchStream.DisplayName} is still live");
            }
        }

        private static void OnStreamOffline(object sender, OnStreamOfflineArgs streamOffline) {
            HostingMonitor.StartMonitor();

            IsLive = false;

            DateTime? startedAt = DatabaseManager.ActiveStreams.Delete(SettingsManager.Configuration.TwitchStream.Username);
            _ = Discord.DiscordClient.SetStatus();

            string duration = startedAt.HasValue ? $"They were live for {(DateTime.UtcNow - startedAt.Value).TotalMilliseconds.Milliseconds().Humanize(3)}{Environment.NewLine}" : "";
            _ = Discord.DiscordClient.TwitchChannelChange(SettingsManager.Configuration.TwitchStream.DiscordChannel, SettingsManager.Configuration.TwitchStream.Url, null, $"{SettingsManager.Configuration.TwitchStream.DisplayName} is offline", $"{duration}Thanks for watching!");

            LoggingManager.Log.Info($"{SettingsManager.Configuration.TwitchStream.DisplayName} is offline");
        }

        private static void OnConnected(object sender, Client.Events.OnConnectedArgs connected) => LoggingManager.Log.Info("Connected to Twitch");

        private static void OnDisconnected(object sender, OnDisconnectedEventArgs disconnected) => LoggingManager.Log.Info("Disconnected from Twitch");

        private static void OnReconnected(object sender, OnReconnectedEventArgs reconnected) => LoggingManager.Log.Info("Reconnected to Twitch");

        private static void OnJoinedChannel(object sender, Client.Events.OnJoinedChannelArgs joined) => LoggingManager.Log.Info($"Joined Twitch channel: {joined.Channel}");

    }

}
