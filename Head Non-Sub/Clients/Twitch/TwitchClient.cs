using System;
using System.Collections.Generic;
using System.Linq;
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

        public static async Task ConnectApiAsync() {
            try {
                LoggingManager.Log.Info("Connecting");

                _ApiSettings = new ApiSettings {
                    ClientId = Constants.ApplicationNameFormatted,
                    AccessToken = SettingsManager.Configuration.TwitchToken
                };

                _TwitchApi = new TwitchAPI(settings: _ApiSettings);

                LoggingManager.Log.Info("Connected");

                // Get user id's for streams that do not have it set
                await GetUserIdsFromUsernamesAsync();

                StartMonitor();

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

                _TwitchClient.Initialize(_ConnectionCredentials, "paymoneywubby");

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
        /// Get 100 most recent clips from the stream.
        /// </summary>
        /// <param name="name">Broadcaster's username or display name</param>
        public static async Task<List<TwitchEntities.Clip>> GetClipsAsync(string name) {
            string userId = SettingsManager.Configuration.TwitchStreams.Where(x => x.UsernameLowercase == name.ToLower()).Select(x => x.UserId).FirstOrDefault();

            if (!string.IsNullOrEmpty(userId)) {
                List<TwitchEntities.Clip> returnClips = new List<TwitchEntities.Clip>();

                GetClipResponse result = await _TwitchApi.Helix.Clips.GetClipAsync(broadcasterId: userId, first: 100);
                foreach (Clip clip in result.Clips) {
                    DateTime created = DateTime.Parse(clip.CreatedAt);

                    returnClips.Add(new TwitchEntities.Clip(DateTime.Parse(clip.CreatedAt), clip.Title, clip.ViewCount, clip.Url));
                }

                LoggingManager.Log.Info($"Retrieved {returnClips.Count} for {name}");

                return returnClips;
            } else {
                throw new UnsupportedTwitchChannelException($"{name} is not a supported Twitch channel at this time.");
            }
        }

        private static async Task GetUserIdsFromUsernamesAsync() {
            List<string> emptyUserIds = SettingsManager.Configuration.TwitchStreams.Where(x => string.IsNullOrWhiteSpace(x.UserId)).Select(x => x.UsernameLowercase).ToList();

            if (emptyUserIds.Count > 0) {
                try {
                    GetUsersResponse result = await _TwitchApi.Helix.Users.GetUsersAsync(logins: emptyUserIds);
                    foreach (User user in result.Users) {
                        SettingsManager.Configuration.TwitchStreams.Where(x => x.UsernameLowercase == user.Login.ToLower()).ToList().ForEach(x => x.UserId = user.Id);
                    }

                    SettingsManager.Save();

                    LoggingManager.Log.Info($"Retrieved {result.Users.Count()} new user ids");
                } catch (Exception ex) {
                    LoggingManager.Log.Error(ex);
                }
            }
        }

        private static void StartMonitor() {
            try {
                _StreamMonitor = new LiveStreamMonitorService(_TwitchApi, 30);
                _StreamMonitor.SetChannelsByName(SettingsManager.Configuration.TwitchStreams.Select(x => x.UsernameLowercase).ToList());

                _StreamMonitor.Start();

                _StreamMonitor.OnStreamOnline += OnStreamOnline;
                _StreamMonitor.OnStreamOffline += OnStreamOffline;

                LoggingManager.Log.Info($"Stream monitoring is running for: {string.Join(", ", SettingsManager.Configuration.TwitchStreams.Select(x => $"{x.DisplayName} ({x.UserId})").ToList())}");
            } catch (Exception ex) {
                LoggingManager.Log.Error(ex);
            }
        }

        private static void OnStreamOnline(object sender, OnStreamOnlineArgs streamOnline) {
            TwitchStream stream = SettingsManager.Configuration.TwitchStreams.Where(x => x.UsernameLowercase == streamOnline.Channel.ToLower()).FirstOrDefault();

            if (DatabaseManager.ActiveStreams.Insert(stream.UsernameLowercase)) {
                if (stream.UsernameLowercase == "paymoneywubby") {
                    _ = Discord.DiscordClient.SetStatus($"Watching PaymoneyWubby!", $"https://twitch.tv/paymoneywubby");
                    _ = Discord.DiscordClient.TwitchChannelChange(stream.DiscordChannel, stream.StreamUrl, streamOnline.Stream.ThumbnailUrl, $"{stream.DisplayName} is now live!", streamOnline.Stream.Title, true);
                } else {
                    _ = Discord.DiscordClient.TwitchChannelChange(stream.DiscordChannel, stream.StreamUrl, streamOnline.Stream.ThumbnailUrl, $"{stream.DisplayName} is now live!", streamOnline.Stream.Title);
                }

                LoggingManager.Log.Info($"{stream.DisplayName} just went live");
            } else {
                if (stream.UsernameLowercase == "paymoneywubby") {
                    _ = Discord.DiscordClient.SetStatus($"Watching PaymoneyWubby!", $"https://twitch.tv/paymoneywubby");
                }

                LoggingManager.Log.Info($"{stream.DisplayName} is still live");
            }
        }

        private static async void OnStreamOffline(object sender, OnStreamOfflineArgs streamOffline) {
            TwitchStream stream = SettingsManager.Configuration.TwitchStreams.Where(x => x.UsernameLowercase == streamOffline.Channel.ToLower()).FirstOrDefault();

            DateTime? startedAt = DatabaseManager.ActiveStreams.Delete(stream.UsernameLowercase);

            if (stream.UsernameLowercase == "paymoneywubby") {
                _ = Discord.DiscordClient.SetStatus();
            }

            string hostingDisplayName = "";
            try {
                // Do not use '_TwitchApi.Undocumented.GetChannelHostsAsync' it is outdated.
                string hostsJson = await Http.SendRequestAsync($"https://tmi.twitch.tv/hosts?include_logins=1&host={stream.UserId}");
                TwitchEntities.HostsResponse hostsResponse = Http.DeserializeJson<TwitchEntities.HostsResponse>(hostsJson);

                if (hostsResponse.Hosts.Count > 0) {
                    hostingDisplayName = hostsResponse.Hosts[0].TargetDisplayName;
                }
            } catch (Exception ex) {
                LoggingManager.Log.Error(ex);
            }

            string duration = startedAt.HasValue ? $"They were live for {(DateTime.UtcNow - startedAt.Value).TotalMilliseconds.Milliseconds().Humanize(3)}{Environment.NewLine}" : "";
            string hosting = !string.IsNullOrEmpty(hostingDisplayName) ? $"{Environment.NewLine}{Environment.NewLine}Raided [{hostingDisplayName}](https://www.twitch.tv/{hostingDisplayName} \"Clicking this link will take you to: https://www.twitch.tv/{hostingDisplayName}\")" : "";

            _ = Discord.DiscordClient.TwitchChannelChange(stream.DiscordChannel, stream.StreamUrl, null, $"{stream.DisplayName} is now offline", $"{duration}Thanks for watching{hosting}");

            LoggingManager.Log.Info($"{stream.DisplayName} is now offline");
        }

        private static void OnConnected(object sender, Client.Events.OnConnectedArgs connected) => LoggingManager.Log.Info("Connected to Twitch");

        private static void OnDisconnected(object sender, OnDisconnectedEventArgs disconnected) => LoggingManager.Log.Info("Disconnected from Twitch");

        private static void OnReconnected(object sender, OnReconnectedEventArgs reconnected) => LoggingManager.Log.Info("Reconnected to Twitch");

        private static void OnJoinedChannel(object sender, Client.Events.OnJoinedChannelArgs joined) => LoggingManager.Log.Info($"Joined Twitch channel: {joined.Channel}");

    }

}
