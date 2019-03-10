using System;
using System.Collections.Generic;
using System.Linq;
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

        private static Dictionary<string, string> _UserIds = new Dictionary<string, string>();
        private static readonly Dictionary<string, string> _StreamNames = new Dictionary<string, string>() {
            { "paymoneywubby", "PaymoneyWubby" }
        };

        public static void ConnectApi() {
            try {
                LoggingManager.Log.Info("Connecting");

                _ApiSettings = new ApiSettings {
                    ClientId = Constants.ApplicationNameFormatted,
                    AccessToken = SettingsManager.Configuration.TwitchToken
                };

                _TwitchApi = new TwitchAPI(settings: _ApiSettings);

                _StreamMonitor = new LiveStreamMonitorService(_TwitchApi, 20);
                _StreamMonitor.SetChannelsByName(_StreamNames.Keys.ToList());

                _StreamMonitor.Start();

                _StreamMonitor.OnStreamOnline += OnStreamOnline;
                _StreamMonitor.OnStreamUpdate += OnStreamUpdate;
                _StreamMonitor.OnStreamOffline += OnStreamOffline;

                LoggingManager.Log.Info("Connected");

                GetUserIdsFromNames();

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
                _TwitchClient.OnMessageReceived += OnMessageReceived;

                LoggingManager.Log.Info("Connected");

            } catch (Exception ex) {
                LoggingManager.Log.Error(ex);
            }
        }

        /// <summary>
        /// Get 100 most recent clips from the stream.
        /// </summary>
        /// <param name="name">Broadcaster's name</param>
        /// <param name="count">Clips to return. Maximum: 100</param>
        public static List<(DateTime createdAt, string title, int viewCount, string url)> GetClips(string name, int count = 100) {
            try {
                string userId = _UserIds.Where(x => x.Key == name.ToLower()).Select(x => x.Value).FirstOrDefault();
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
                    return null;
                }
            } catch (Exception ex) {
                LoggingManager.Log.Error(ex);
                return null;
            }
        }

        private static void GetUserIdsFromNames() {
            try {
                User[] users = _TwitchApi.Helix.Users.GetUsersAsync(logins: _StreamNames.Select(x => x.Key).ToList()).Result.Users;
                foreach (User user in users) {
                    _UserIds.Add(user.Login.ToLower(), user.Id);
                }

                LoggingManager.Log.Info($"Retrieved {_UserIds.Count} user ids");
            } catch (Exception ex) {
                LoggingManager.Log.Error(ex);
            }
        }

        private static void OnStreamOnline(object sender, OnStreamOnlineArgs e) {
            string channel = _StreamNames.Where(x => x.Key.ToLower() == e.Channel.ToLower()).FirstOrDefault().Value;

            _ = Discord.DiscordClient.SetStatus($"Watching {channel}!", $"https://twitch.tv/{e.Channel}");
            _ = Discord.DiscordClient.TwitchChannelChange(channel, e.Stream.ThumbnailUrl, $"{channel} is now live!", e.Stream.Title);

            LoggingManager.Log.Info($"{channel} just went live");
        }

        private static void OnStreamUpdate(object sender, OnStreamUpdateArgs e) {

        }

        private static void OnStreamOffline(object sender, OnStreamOfflineArgs e) {
            string channel = _StreamNames.Where(x => x.Key.ToLower() == e.Channel.ToLower()).FirstOrDefault().Value;

            _ = Discord.DiscordClient.SetStatus();
            _ = Discord.DiscordClient.TwitchChannelChange(channel, null, $"{channel} is now offline", "Thanks for watching");

            LoggingManager.Log.Info($"{channel} is now offline");
        }

        private static void OnConnected(object sender, Client.Events.OnConnectedArgs e) {
            LoggingManager.Log.Info("Connected to Twitch");
        }

        private static void OnDisconnected(object sender, OnDisconnectedEventArgs e) {
            LoggingManager.Log.Info("Disconnected from Twitch");
        }

        private static void OnReconnected(object sender, OnReconnectedEventArgs e) {
            LoggingManager.Log.Info("Reconnected to Twitch");
        }

        private static void OnJoinedChannel(object sender, Client.Events.OnJoinedChannelArgs e) {
            LoggingManager.Log.Info($"Joined Twitch channel: {e.Channel}");
        }

        private static void OnMessageReceived(object sender, Client.Events.OnMessageReceivedArgs e) {

        }

    }

}
