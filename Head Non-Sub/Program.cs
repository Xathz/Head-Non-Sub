using System;
using System.IO;
using System.Threading.Tasks;
using HeadNonSub.HostedServices;
using HeadNonSub.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NLog;
using NLog.Extensions.Logging;

namespace HeadNonSub {

    class Program {

        static async Task Main(string[] args) {
            try {
                CreatePIDFile();
                Console.Title = Constants.ApplicationName;

                // Create and run the host
                IHost host = CreateHostBuilder(args).Build();
                Shutdown.SetLifetime(host.Services.GetRequiredService<IHostApplicationLifetime>());
                await host.RunAsync();
            } catch (Exception ex) {
                LoggingManager.Log.Error(ex, "Application terminated with error");
                Environment.ExitCode = Shutdown.RestartExitCode;
            } finally {
                LoggingManager.Flush();
                // Ensure all appenders are flushed
                LogManager.Shutdown();
            }
        }

        static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, config) => {
                    config
                        .SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false);
                })
                .ConfigureLogging((context, logging) => {
                    // Only NLog so every console line uses the same format, configured in LoggingManager
                    logging.ClearProviders();
                    logging.AddNLog();
                })
                .ConfigureServices((context, services) => {
                    // Ensure directories exist
                    EnsureDirectories();

                    // Initialize logging and settings
                    LoggingManager.Initialize();
                    LoggingManager.Log.Info($"{Constants.ApplicationName} v{Constants.ApplicationVersion}, https://github.com/Xathz");
                    Shutdown.ClearFailFastFile();
                    SettingsManager.Load();

                    // Register configuration
                    services.AddSingleton(sp => Options.Create(SettingsManager.Configuration));

                    // Register hosted services (order matters)
                    services.AddHostedService<CacheHostedService>();
                    services.AddHostedService<BotHostedService>();

                    LoggingManager.Log.Info("Services configured");
                });

        static void EnsureDirectories() {
            Directory.CreateDirectory(Constants.WorkingDirectory);
            Directory.CreateDirectory(Constants.LogDirectory);
            Directory.CreateDirectory(Constants.TemporaryDirectory);
            Directory.CreateDirectory(Constants.MagickNETDirectory);
            Directory.CreateDirectory(Constants.ContentDirectory);
        }

        static void CreatePIDFile() {
            try {
                File.WriteAllText(Constants.ProcessIdFile, Constants.ProcessId.ToString());
            } catch (Exception ex) {
                LoggingManager.Log.Error(ex);
            }
        }

    }

}
