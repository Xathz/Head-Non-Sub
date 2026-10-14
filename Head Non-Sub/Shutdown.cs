using System;
using Microsoft.Extensions.Hosting;

namespace HeadNonSub {

    /// <summary>
    /// Stops the application when it can not keep running. The exit code tells Watchdog.ps1 what to do.
    /// </summary>
    public static class Shutdown {

        /// <summary>
        /// Exit code when the watchdog should restart the application.
        /// </summary>
        public const int RestartExitCode = 1;

        /// <summary>
        /// Exit code when the application needs to be fixed before it can run again.
        /// </summary>
        public const int FailFastExitCode = 2;

        private static readonly object _Lock = new object();
        private static IHostApplicationLifetime _Lifetime;
        private static bool _Stopping;

        /// <summary>
        /// Set the host lifetime used to stop the application.
        /// </summary>
        /// <param name="lifetime">Lifetime of the running host.</param>
        public static void SetLifetime(IHostApplicationLifetime lifetime) => _Lifetime = lifetime;

        /// <summary>
        /// Stop for a problem that needs a settings or setup change. The watchdog does not restart it.
        /// </summary>
        /// <param name="reason">What needs to be fixed.</param>
        public static void FailFast(string reason) {
            if (!BeginStop()) { return; }

            LoggingManager.Log.Fatal($"{reason}. Stopping, fix this then start it again");
            Stop(FailFastExitCode);
        }

        /// <summary>
        /// Stop so the watchdog starts the application again.
        /// </summary>
        /// <param name="reason">Why it is restarting.</param>
        public static void Restart(string reason) {
            if (!BeginStop()) { return; }

            LoggingManager.Log.Fatal($"{reason}. Stopping so the watchdog can restart it");
            Stop(RestartExitCode);
        }

        private static bool BeginStop() {
            lock (_Lock) {
                // Only the first reason counts, later ones are usually caused by the same problem
                if (_Stopping) { return false; }

                _Stopping = true;
                return true;
            }
        }

        private static void Stop(int exitCode) {
            Environment.ExitCode = exitCode;

            if (_Lifetime != null) {
                _Lifetime.StopApplication();
            } else {
                // Host is not running yet
                LoggingManager.Flush();
                Environment.Exit(exitCode);
            }
        }

    }

}
