using System;
using System.IO;
using Microsoft.Extensions.Hosting;

namespace HeadNonSub {

    /// <summary>
    /// Stops the application when it can not keep running. The exit code and fail fast file tell Watchdog.ps1 what to do.
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
        /// Stop for a problem that needs a settings or setup change. Writes the fail fast file so the watchdog does not keep restarting.
        /// </summary>
        /// <param name="reason">What needs to be fixed.</param>
        public static void FailFast(string reason) {
            if (!BeginStop()) { return; }

            LoggingManager.Log.Fatal($"{reason}. Stopping, fix this then delete '{Constants.FailFastFile}' so the watchdog can start it again");
            WriteFailFastFile(reason);
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

        /// <summary>
        /// Write the fail fast file, the watchdog will not start the application while it exists.
        /// </summary>
        /// <param name="reason">What needs to be fixed.</param>
        public static void WriteFailFastFile(string reason) {
            try {
                File.WriteAllText(Constants.FailFastFile, $"{DateTime.Now.ToString(Constants.DateTimeFormatMedium)} {reason}");
            } catch (Exception ex) {
                LoggingManager.Log.Error(ex, "Failed to write the fail fast file");
            }
        }

        /// <summary>
        /// Remove a fail fast file left from the last run. Starting the application means the problem was fixed.
        /// </summary>
        public static void ClearFailFastFile() {
            try {
                if (File.Exists(Constants.FailFastFile)) {
                    LoggingManager.Log.Info($"Removed the fail fast file from the last run: {File.ReadAllText(Constants.FailFastFile)}");
                    File.Delete(Constants.FailFastFile);
                }
            } catch (Exception ex) {
                LoggingManager.Log.Error(ex, "Failed to remove the fail fast file");
            }
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
