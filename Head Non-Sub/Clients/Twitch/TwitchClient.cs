using System;
using System.Collections.Generic;
using HeadNonSub.Settings;
using TwitchLib.Api;
using TwitchLib.Api.Core;
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
                _ApiSettings = new ApiSettings {
                    ClientId = Constants.ApplicationNameFormatted,
                    AccessToken = SettingsManager.Configuration.TwitchToken
                };

                _TwitchApi = new TwitchAPI(settings: _ApiSettings);

                _StreamMonitor = new LiveStreamMonitorService(_TwitchApi, 20);
                _StreamMonitor.SetChannelsByName(new List<string> { "paymoneywubby" });

                _StreamMonitor.Start();

                _StreamMonitor.OnStreamOnline += OnStreamOnline;
                _StreamMonitor.OnStreamUpdate += OnStreamUpdate;
                _StreamMonitor.OnStreamOffline += OnStreamOffline;

            } catch (Exception ex) {
                LoggingManager.Log.Error(ex);
            }
        }

        public static void ConnectClient() {
            try {
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

            } catch (Exception ex) {
                LoggingManager.Log.Error(ex);
            }
        }

        //private static Dictionary<string, (string Id, string DisplayName, string ProfileImageUrl)> _UserIds = new Dictionary<string, (string Id, string DisplayName, string ProfileImageUrl)>();

        //private static void GetUsers() {
        //    User[] users = _TwitchApi.Helix.Users.GetUsersAsync(null, new List<string> { "" }).Result.Users;
        //    foreach (User user in users) {
        //        _UserIds.Add(user.Login, (user.Id, user.DisplayName, user.ProfileImageUrl));
        //    }
        //}

        private static void OnStreamOnline(object sender, OnStreamOnlineArgs e) {
            _ = Discord.DiscordClient.TwitchChannelChange(e.Channel, e.Stream.ThumbnailUrl, $"{e.Channel} is now live!", e.Stream.Title);
        }

        private static void OnStreamUpdate(object sender, OnStreamUpdateArgs e) {
            
        }

        private static void OnStreamOffline(object sender, OnStreamOfflineArgs e) {
            _ = Discord.DiscordClient.TwitchChannelChange(e.Channel, e.Stream.ThumbnailUrl, $"{e.Channel} is now offline", "Thanks for watching");
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
