using System;
using System.Collections.Generic;
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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace HeadNonSub {

    class Program {

        private readonly DateTime _Started = DateTime.Now;
        private readonly IServiceProvider _ServiceProvider;

        static void Main() => new Program().StartAsync().GetAwaiter().GetResult();

        public Program() {
            CreatePIDFile();
            Console.Title = Constants.ApplicationName;

            PrintBanner();

            Directory.CreateDirectory(Constants.WorkingDirectory);
            Directory.CreateDirectory(Constants.LogDirectory);
            Directory.CreateDirectory(Constants.RuntimesDirectory);
            Directory.CreateDirectory(Constants.TemporaryDirectory);
            Directory.CreateDirectory(Constants.MagickNETDirectory);
            Directory.CreateDirectory(Constants.ContentDirectory);

            LoggingManager.Initialize();
            SettingsManager.Load();

            // Setup dependency injection container
            ServiceCollection services = new ServiceCollection();
            ConfigureServices(services);
            _ServiceProvider = services.BuildServiceProvider();

            // Pass configuration to static services that need it
            IOptions<Configuration> configOptions = _ServiceProvider.GetRequiredService<IOptions<Configuration>>();
            SettingsManager.SetServiceProvider(_ServiceProvider);
            DiscordClient.SetConfigurationOptions(configOptions);
            TwitchClient.SetConfigurationOptions(configOptions);
            Backblaze.SetConfigurationOptions(configOptions);
            Http.SetConfigurationOptions(configOptions);
            Clients.Twitch.HostingMonitor.SetConfigurationOptions(configOptions);

            // Validate configuration
            Configuration configuration = configOptions.Value;
            List<string> validationErrors = ConfigurationValidator.Validate(configuration);
            if (validationErrors.Count > 0) {
                foreach (string error in validationErrors) {
                    LoggingManager.Log.Error(error);
                }
                throw new InvalidOperationException("Configuration validation failed");
            }

            DatabaseManager.Load();
            StatisticsManager.Load();

            MagickNET.SetTempDirectory(Constants.MagickNETDirectory);

            // Increase humanizer's precision 
            Configurator.DateTimeHumanizeStrategy = new PrecisionDateTimeHumanizeStrategy(precision: .95);
            Configurator.DateTimeOffsetHumanizeStrategy = new PrecisionDateTimeOffsetHumanizeStrategy(precision: .95);
        }

        /// <summary>
        /// Configure dependency injection services.
        /// </summary>
        private static void ConfigureServices(IServiceCollection services) {
            // Register configuration as a singleton
            services.AddSingleton<IOptions<Configuration>>(sp => {
                return Options.Create(SettingsManager.Configuration);
            });

            // Register other services here as needed
            LoggingManager.Log.Info("Services configured");
        }

        private static void PrintBanner() {
            ConsoleColor originalColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"===================================");
            Console.WriteLine($"======= {Constants.ApplicationName} v{Constants.ApplicationVersion} =======");
            Console.WriteLine("===   https://github.com/Xathz  ===");
            Console.WriteLine($"===================================");
            Console.ForegroundColor = originalColor;
            Console.WriteLine();
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

        private static void CreatePIDFile() {
            try {
                File.WriteAllText(Constants.ProcessIdFile, Constants.ProcessId.ToString());
            } catch (Exception ex) {
                LoggingManager.Log.Error(ex);
            }
        }

    }

}
