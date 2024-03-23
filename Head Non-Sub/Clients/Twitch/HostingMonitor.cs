using System;
using System.Timers;
using HeadNonSub.Settings;
using TwitchEntities = HeadNonSub.Entities.Twitch;

namespace HeadNonSub.Clients.Twitch {

    public static class HostingMonitor {

        private static Timer _Timer;
        private static volatile bool _Stop = false;

        private const int _MaxCount = 25;
        private static volatile int _Count = 0;

        public static void StartMonitor() {
            _Timer = new Timer();
            _Stop = false;
            _Count = 0;

            _Timer.Interval = 20000;
            _Timer.Elapsed += Check;
            _Timer.AutoReset = false;

            _Timer.Start();

            LoggingManager.Log.Info($"Started monitor for: {SettingsManager.Configuration.TwitchStream.DisplayName}");
        }

        public static void StopMonitor() {
            _Stop = true;
            if (_Timer is Timer) {
                _Timer.Stop();
            }

            LoggingManager.Log.Info($"Stopped monitor for: {SettingsManager.Configuration.TwitchStream.DisplayName}");
        }

        private static async void Check(object sender, ElapsedEventArgs elapsed) {
            if (!_Stop) {
                if (_Count >= _MaxCount) {
                    LoggingManager.Log.Info($"Check max count reached, stopping monitor for: {SettingsManager.Configuration.TwitchStream.DisplayName}");
                    StopMonitor();
                    return;
                }

                try {
                    string hostsJson = await Http.SendRequestAsync($"https://tmi.twitch.tv/hosts?include_logins=1&host={SettingsManager.Configuration.TwitchStream.UserId}");
                    TwitchEntities.HostsResponse hostsResponse = Http.DeserializeJson<TwitchEntities.HostsResponse>(hostsJson);

                    if (hostsResponse.Hosts.Count > 0 && !string.IsNullOrEmpty(hostsResponse.Hosts[0].TargetDisplayName)) {
                        string host = hostsResponse.Hosts[0].TargetDisplayName;

                        await Discord.DiscordClient.TwitchChannelChange(SettingsManager.Configuration.TwitchStream.DiscordChannel, $"https://www.twitch.tv/{host}", null, $"{SettingsManager.Configuration.TwitchStream.DisplayName} is hosting {host}", null);

                        LoggingManager.Log.Info($"Now hosting {host}, stopping monitor for: {SettingsManager.Configuration.TwitchStream.DisplayName}");
                        StopMonitor();
                        return;
                    }

                } catch (Exception ex) {
                    LoggingManager.Log.Error(ex);
                }

                _Count = _Count++;
                _Timer.Start();
            }
        }

    }

}
