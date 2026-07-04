using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace HeadNonSub.HostedServices {

    /// <summary>
    /// Hosted service that loads content into the cache during application startup.
    /// </summary>
    public class CacheHostedService : IHostedService {

        public async Task StartAsync(CancellationToken cancellationToken) {
            LoggingManager.Log.Info("Initializing cache...");
            await Cache.LoadContentAsync();
            LoggingManager.Log.Info("Cache initialization complete");
        }

        public Task StopAsync(CancellationToken cancellationToken) {
            LoggingManager.Log.Info("Cache service stopping");
            return Task.CompletedTask;
        }

    }

}
