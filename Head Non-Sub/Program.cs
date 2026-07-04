using System;
using System.IO;
using System.Threading.Tasks;
using HeadNonSub.HostedServices;
using HeadNonSub.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using NLog;
using NLog.Extensions.Logging;

namespace HeadNonSub {

    class Program {

        static async Task Main(string[] args) {
            try {
                CreatePIDFile();
                Console.Title = Constants.ApplicationName;
                PrintBanner();

                // Create and run the host
                IHost host = CreateHostBuilder(args).Build();
                await host.RunAsync();
            } catch (Exception ex) {
                LoggingManager.Log.Error(ex, "Application terminated with error");
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
                    // Add NLog provider for logging
                    logging.AddNLog("nlog.config");
                })
                .ConfigureServices((context, services) => {
                    // Ensure directories exist
                    EnsureDirectories();

                    // Initialize logging and settings
                    LoggingManager.Initialize();
                    SettingsManager.Load();

                    // Register configuration
                    services.AddSingleton(sp => Options.Create(SettingsManager.Configuration));

                    // Register hosted services (order matters)
                    services.AddHostedService<CacheHostedService>();
                    services.AddHostedService<BotHostedService>();

                    // Register memory cache
                    services.AddMemoryCache();

                    // Expose IServiceProvider to static services
                    services.AddSingleton(sp => sp);

                    LoggingManager.Log.Info("Services configured");
                });

        static void EnsureDirectories() {
            Directory.CreateDirectory(Constants.WorkingDirectory);
            Directory.CreateDirectory(Constants.LogDirectory);
            Directory.CreateDirectory(Constants.RuntimesDirectory);
            Directory.CreateDirectory(Constants.TemporaryDirectory);
            Directory.CreateDirectory(Constants.MagickNETDirectory);
            Directory.CreateDirectory(Constants.ContentDirectory);
        }

        static void PrintBanner() {
            ConsoleColor originalColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"===================================");
            Console.WriteLine($"======= {Constants.ApplicationName} v{Constants.ApplicationVersion} =======");
            Console.WriteLine("===   https://github.com/Xathz  ===");
            Console.WriteLine($"===================================");
            Console.ForegroundColor = originalColor;
            Console.WriteLine();
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
