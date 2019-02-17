using System;
using System.IO;
using System.Threading.Tasks;
using HeadNonSub.Clients.Discord;
using HeadNonSub.Clients.Twitch;
using HeadNonSub.Settings;

namespace HeadNonSub {

    class Program {

        private readonly DateTime _Started = DateTime.Now;

        static void Main(string[] args) => new Program().StartAsync().GetAwaiter().GetResult();

        public Program() {
            Console.Title = Constants.ApplicationName;

            ConsoleColor originalColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"===================================");
            Console.WriteLine($"======= {Constants.ApplicationName} v{Constants.ApplicationVersion} =======");
            Console.WriteLine("===   https://github.com/Xathz  ===");
            Console.WriteLine($"===================================");
            Console.ForegroundColor = originalColor;

            Console.WriteLine();

            Directory.CreateDirectory(Constants.WorkingDirectory);
            Directory.CreateDirectory(Constants.LogDirectory);
            Directory.CreateDirectory(Constants.TemporaryDirectory);
            Directory.CreateDirectory(Constants.ContentDirectory);

            LoggingManager.Initialize();
            SettingsManager.Load();
        }

        private async Task StartAsync() {
            LoggingManager.Log.Info("Starting...");

            // Connect to discord
            await DiscordClient.ConnectAsync();

            // Connect to twitch
            //TwitchClient.Connect();

            // Block and wait
            await UserInputAsync();
        }

        private async Task UserInputAsync() {
            WaitForInput:

            string input = await Task.Run(() => Console.ReadLine());
            if (input == "exit") {
                LoggingManager.Log.Info("Exiting...");

                await DiscordClient.StopAsync();
                await Task.Delay(2000);

                return;
            } else if (input.StartsWith("message")) {
                try {
                    string[] args = input.Split(' ');
                    string message = input.Replace(args[0], "").Replace(args[1], "").Trim();

                    if (args.Length >= 2) {
                        ulong? reply = DiscordClient.SendMessageToChannelAsync(ulong.Parse(args[1]), message).Result;
                        if (reply.HasValue) {
                            Console.WriteLine($"Message was sent: {reply.Value}");
                        } else {
                            Console.WriteLine("Message was not set, the reply message id was null.");
                        }
                    } else {
                        Console.WriteLine("Check the command and try again. Example: message 0000000000000000 Some message!");
                    }
                } catch { }

            } else if (input == "help" || input == string.Empty) {
                Console.WriteLine($"=== {Constants.ApplicationName} v{Constants.ApplicationVersion}; Running for: {DateTime.Now.Subtract(_Started).ToString("c")}");
                Console.WriteLine($"=== Available commands: exit, message <channelId>");

            }

            goto WaitForInput;
        }

    }

}
