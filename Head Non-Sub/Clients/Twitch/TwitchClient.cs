using System;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Events;
using Client = TwitchLib.Client;

namespace HeadNonSub.Clients.Twitch {

    public static class TwitchClient {

        private static ConnectionCredentials _ConnectionCredentials;
        private static Client.TwitchClient _TwitchClient;

        public static void Connect() {
            try {
                _ConnectionCredentials = new ConnectionCredentials("", "");

                _TwitchClient = new Client.TwitchClient {
                    AutoReListenOnException = true
                };

                _TwitchClient.Initialize(_ConnectionCredentials, "paymoneywubby");


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
            //_ = DiscordClient.SendMessageToChannelAsync(537727672747294738, $"[{e.ChatMessage.Username}] {e.ChatMessage.Message}");
        }

    }

}
