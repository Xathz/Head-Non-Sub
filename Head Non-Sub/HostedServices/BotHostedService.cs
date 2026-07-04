using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HeadNonSub.Clients.Discord;
using HeadNonSub.Clients.Twitch;
using HeadNonSub.Database;
using HeadNonSub.Settings;
using HeadNonSub.Statistics;
using Humanizer;
using ImageMagick;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace HeadNonSub.HostedServices {

    /// <summary>
    /// Hosted service that orchestrates bot initialization including Discord and Twitch connections.
    /// </summary>
    public class BotHostedService : IHostedService {

        private readonly IHostApplicationLifetime _ApplicationLifetime;
        private readonly IConfiguration _Configuration;
        private readonly IOptions<Configuration> _ConfigurationOptions;
        private readonly IServiceProvider _ServiceProvider;
        private DateTime _Started;

        public BotHostedService(IHostApplicationLifetime applicationLifetime, IConfiguration configuration, IOptions<Configuration> configurationOptions, IServiceProvider serviceProvider) {
            _ApplicationLifetime = applicationLifetime;
            _Configuration = configuration;
            _ConfigurationOptions = configurationOptions;
            _ServiceProvider = serviceProvider;
        }

        public async Task StartAsync(CancellationToken cancellationToken) {
            _Started = DateTime.Now;
            LoggingManager.Log.Info("Bot service starting...");

            try {
                // Pass service provider to static services
                SettingsManager.SetServiceProvider(_ServiceProvider);
                DiscordClient.SetConfigurationOptions(_ConfigurationOptions);
                TwitchClient.SetConfigurationOptions(_ConfigurationOptions);
                Backblaze.SetConfigurationOptions(_ConfigurationOptions);
                Http.SetConfigurationOptions(_ConfigurationOptions);
                HostingMonitor.SetConfigurationOptions(_ConfigurationOptions);

                // Configure Magick.NET
                MagickNET.SetTempDirectory(Constants.MagickNETDirectory);

                // Configure Humanizer
                Configurator.DateTimeHumanizeStrategy = new PrecisionDateTimeHumanizeStrategy(precision: .95);
                Configurator.DateTimeOffsetHumanizeStrategy = new PrecisionDateTimeOffsetHumanizeStrategy(precision: .95);

                // Validate configuration
                Configuration configuration = _ConfigurationOptions.Value;
                List<string> validationErrors = ConfigurationValidator.Validate(configuration);
                if (validationErrors.Count > 0) {
                    foreach (string error in validationErrors) {
                        LoggingManager.Log.Error(error);
                    }

                    _ApplicationLifetime.StopApplication();
                    return;
                }

                // Load database managers
                DatabaseManager.Load();
                StatisticsManager.Load();

                // Connect to Discord
                await DiscordClient.ConnectAsync();

                // Connect to Twitch
                await TwitchClient.ConnectApiAsync();

                LoggingManager.Log.Info("Bot service started successfully");

                // Start the console input loop
                _ = UserInputAsync(cancellationToken);
            } catch (Exception ex) {
                LoggingManager.Log.Error(ex, "Failed to start bot service");
                _ApplicationLifetime.StopApplication();
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken) {
            LoggingManager.Log.Info("Bot service stopping...");
            await DiscordClient.StopAsync();
            await Task.Delay(2000, cancellationToken);
            LoggingManager.Log.Info("Bot service stopped");
        }

        private async Task UserInputAsync(CancellationToken cancellationToken) {
            try {
                while (!cancellationToken.IsCancellationRequested) {
                    string input = await Task.Run(() => Console.ReadLine(), cancellationToken);

                    if (input == "exit") {
                        LoggingManager.Log.Info("Exit command received");
                        _ApplicationLifetime.StopApplication();
                        break;
                    } else if (input == "cache") {
                        LoggingManager.Log.Info($"Keys in the cache: {Cache.ListKeys()}");
                    } else if (input == "help" || string.IsNullOrWhiteSpace(input)) {
                        TimeSpan uptime = DateTime.Now.Subtract(_Started);
                        Console.WriteLine($"=== {Constants.ApplicationName} v{Constants.ApplicationVersion}; Running for: {uptime:c}");
                        Console.WriteLine($"=== Available commands: exit, cache, help");
                    }
                }
            } catch (OperationCanceledException) {
                // Expected when application is shutting down
            } catch (Exception ex) {
                LoggingManager.Log.Error(ex, "Error in user input loop");
            }
        }

    }

}
