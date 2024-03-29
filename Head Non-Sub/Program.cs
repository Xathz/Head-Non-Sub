﻿using System;
using System.IO;
using System.Threading.Tasks;
using HeadNonSub.Clients.Discord;
using HeadNonSub.Clients.Twitch;
using HeadNonSub.Database;
using HeadNonSub.Settings;
using HeadNonSub.Statistics;
using Humanizer.Configuration;
using Humanizer.DateTimeHumanizeStrategy;
using ImageMagick;

namespace HeadNonSub {

    class Program {

        private readonly DateTime _Started = DateTime.Now;

        static void Main() => new Program().StartAsync().GetAwaiter().GetResult();

        public Program() {
            CreatePIDFile();
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
            Directory.CreateDirectory(Constants.RuntimesDirectory);
            Directory.CreateDirectory(Constants.TemporaryDirectory);
            Directory.CreateDirectory(Constants.MagickNETDirectory);
            Directory.CreateDirectory(Constants.ContentDirectory);

            LoggingManager.Initialize();
            SettingsManager.Load();

            DatabaseManager.Load();
            StatisticsManager.Load();

            MagickNET.SetTempDirectory(Constants.MagickNETDirectory);

            // Increase humanizer's precision 
            Configurator.DateTimeHumanizeStrategy = new PrecisionDateTimeHumanizeStrategy(precision: .95);
            Configurator.DateTimeOffsetHumanizeStrategy = new PrecisionDateTimeOffsetHumanizeStrategy(precision: .95);
        }

        private async Task StartAsync() {
            LoggingManager.Log.Info("Starting...");

            // Load content into cache
            await Cache.LoadContentAsync();

            // Connect to Discord
            await DiscordClient.ConnectAsync();

            // Connect to Twitch
            await TwitchClient.ConnectApiAsync();

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

                LoggingManager.Flush();
                await Task.Delay(1000);

                return;
            } else if (input == "cache") {
                LoggingManager.Log.Info($"Keys in the cache: {Cache.ListKeys()}");

            } else if (input == "help" || string.IsNullOrWhiteSpace(input)) {
                Console.WriteLine($"=== {Constants.ApplicationName} v{Constants.ApplicationVersion}; Running for: {DateTime.Now.Subtract(_Started):c}");
                Console.WriteLine($"=== Available commands: exit, cache");
            }

            goto WaitForInput;
        }

        private void CreatePIDFile() {
            try {
                File.WriteAllText(Constants.ProcessIdFile, Constants.ProcessId.ToString());
            } catch (Exception ex) {
                LoggingManager.Log.Error(ex);
            }
        }

    }

}
