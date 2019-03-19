using System;
using System.Collections.Generic;
using System.Linq;
using HeadNonSub.Exceptions;
using HeadNonSub.Settings;
using TwitchLib.Api;
using TwitchLib.Api.Core;
using TwitchLib.Api.Helix.Models.Clips.GetClip;
using TwitchLib.Api.Helix.Models.Users;
using TwitchLib.Api.Services;
using TwitchLib.Api.Services.Events.LiveStreamMonitor;
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

        public static void ConnectApi() {
            try {
                LoggingManager.Log.Info("Connecting");

                _ApiSettings = new ApiSettings {
                    ClientId = Constants.ApplicationNameFormatted,
                    AccessToken = SettingsManager.Configuration.TwitchToken
                };

                _TwitchApi = new TwitchAPI(settings: _ApiSettings);

                LoggingManager.Log.Info("Connected");

                // Get user id's for streams that do not have it set
                GetUserIdsFromUsernames();

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

                _TwitchClient.Initialize(_ConnectionCredentials);

                _TwitchClient.Connect();

                _TwitchClient.OnConnected += OnConnected;
                _TwitchClient.OnDisconnected += OnDisconnected;
                _TwitchClient.OnReconnected += OnReconnected;

                _TwitchClient.OnJoinedChannel += OnJoinedChannel;
                //_TwitchClient.OnMessageReceived += OnMessageReceived;

                LoggingManager.Log.Info("Connected");

            } catch (Exception ex) {
                LoggingManager.Log.Error(ex);
            }
        }

        /// <summary>
        /// Get 100 most recent clips from the stream.
        /// </summary>
        /// <param name="name">Broadcaster's username or display name</param>
        /// <param name="count">Clips to return. Maximum: 100</param>
        public static List<(DateTime createdAt, string title, int viewCount, string url)> GetClips(string name, int count = 100) {
            string userId = SettingsManager.Configuration.TwitchStreams.Where(x => x.UsernameLowercase == name.ToLower()).Select(x => x.UserId).FirstOrDefault();

            if (!string.IsNullOrEmpty(userId)) {
                List<(DateTime createdAt, string title, int viewCount, string url)> returnClips =
                    new List<(DateTime createdAt, string title, int viewCount, string url)>();

                Clip[] clips = _TwitchApi.Helix.Clips.GetClipAsync(broadcasterId: userId, first: 100).Result.Clips;
                foreach (Clip clip in clips) {
                    DateTime created = DateTime.Parse(clip.CreatedAt);

                    returnClips.Add((DateTime.Parse(clip.CreatedAt), clip.Title, clip.ViewCount, clip.Url));
                }

                LoggingManager.Log.Info($"Retrieved {returnClips.Count} for {name}");

                return returnClips;
            } else {
                throw new UnsupportedTwitchChannelException($"{name} is not a supported Twitch channel at this time.");
            }
        }

        private static void GetUserIdsFromUsernames() {
            List<string> emptyUserIds = SettingsManager.Configuration.TwitchStreams.Where(x => string.IsNullOrWhiteSpace(x.UserId)).Select(x => x.UsernameLowercase).ToList();

            if (emptyUserIds.Count > 0) {
                try {
                    User[] users = _TwitchApi.Helix.Users.GetUsersAsync(logins: emptyUserIds).Result.Users;
                    foreach (User user in users) {
                        SettingsManager.Configuration.TwitchStreams.Where(x => x.UsernameLowercase == user.Login.ToLower()).ToList().ForEach(x => x.UserId = user.Id);
                    }

                    SettingsManager.Save();

                    LoggingManager.Log.Info($"Retrieved {users.Count()} new user ids");
                } catch (Exception ex) {
                    LoggingManager.Log.Error(ex);
                }
            }
        }

        private static void StartMonitor() {
            _StreamMonitor = new LiveStreamMonitorService(_TwitchApi, 20);
            _StreamMonitor.SetChannelsByName(SettingsManager.Configuration.TwitchStreams.Select(x => x.UsernameLowercase).ToList());

            _StreamMonitor.Start();

            _StreamMonitor.OnStreamOnline += OnStreamOnline;
            _StreamMonitor.OnStreamUpdate += OnStreamUpdate;
            _StreamMonitor.OnStreamOffline += OnStreamOffline;

            LoggingManager.Log.Info($"Stream monitoring is running for: {string.Join(", ", SettingsManager.Configuration.TwitchStreams.Select(x => $"{x.DisplayName} ({x.UserId})").ToList())}");
        }

        private static void OnStreamOnline(object sender, OnStreamOnlineArgs streamOnline) {
            TwitchStream stream = SettingsManager.Configuration.TwitchStreams.Where(x => x.UsernameLowercase == streamOnline.Channel.ToLower()).FirstOrDefault();

            // Wubby only, hes special
            if (streamOnline.Channel.ToLower() == Constants.PaymoneyWubby) {
                _ = Discord.DiscordClient.SetStatus($"Watching PaymoneyWubby!", $"https://twitch.tv/paymoneywubby");
                _ = Discord.DiscordClient.TwitchChannelChange(stream.DiscordChannel, stream.StreamUrl, stream.DisplayName, streamOnline.Stream.ThumbnailUrl, $"{stream.DisplayName} is now live!", streamOnline.Stream.Title, true);
            } else {
                _ = Discord.DiscordClient.TwitchChannelChange(stream.DiscordChannel, stream.StreamUrl, stream.DisplayName, streamOnline.Stream.ThumbnailUrl, $"{stream.DisplayName} is now live!", streamOnline.Stream.Title);
            }

            LoggingManager.Log.Info($"{stream.DisplayName} just went live");
        }

        private static void OnStreamUpdate(object sender, OnStreamUpdateArgs streamUpdate) {

        }

        private static void OnStreamOffline(object sender, OnStreamOfflineArgs streamOffline) {
            TwitchStream stream = SettingsManager.Configuration.TwitchStreams.Where(x => x.UsernameLowercase == streamOffline.Channel.ToLower()).FirstOrDefault();

            // Wubby only, hes special
            if (streamOffline.Channel.ToLower() == Constants.PaymoneyWubby) {
                _ = Discord.DiscordClient.SetStatus();
            }
            _ = Discord.DiscordClient.TwitchChannelChange(stream.DiscordChannel, stream.StreamUrl, stream.DisplayName, null, $"{stream.DisplayName} is now offline", "Thanks for watching");

            LoggingManager.Log.Info($"{stream.DisplayName} is now offline");
        }

        private static void OnConnected(object sender, Client.Events.OnConnectedArgs connected) => LoggingManager.Log.Info("Connected to Twitch");

        private static void OnDisconnected(object sender, OnDisconnectedEventArgs disconnected) => LoggingManager.Log.Info("Disconnected from Twitch");

        private static void OnReconnected(object sender, OnReconnectedEventArgs reconnected) => LoggingManager.Log.Info("Reconnected to Twitch");

        private static void OnJoinedChannel(object sender, Client.Events.OnJoinedChannelArgs joined) => LoggingManager.Log.Info($"Joined Twitch channel: {joined.Channel}");

    }

}
