using System;
using System.IO;
using System.Timers;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace HeadNonSub.Statistics {

    public static class StatisticsManager {

        /// <summary>
        /// Statistics.
        /// </summary>
        public static Statistics Statistics = new Statistics();

        private static Timer _SaveTimer = new Timer(300000); // 5min
        private static void SaveTimerElapsed(object sender, ElapsedEventArgs e) => Save();

        /// <summary>
        /// Load statistics from the disk at <see cref="Constants.StatisticsFile" />.
        /// </summary>
        public static void Load() {
            if (!File.Exists(Constants.StatisticsFile)) {
                LoggingManager.Log.Warn($"Statistics file was not found at '{Constants.StatisticsFile}', creating default one.");
                SaveDefault();
            }

            try {
                LoadJSON(Constants.StatisticsFile);

                // Start save timer
                _SaveTimer.Elapsed += SaveTimerElapsed;
                _SaveTimer.Start();

                LoggingManager.Log.Info("Statistics loaded.");
                return;
            } catch (Exception ex) {
                LoggingManager.Log.Error(ex);
            }

            LoggingManager.Log.Fatal($"Can not load statistics file '{Constants.StatisticsFile}', please check it or delete it so a new one can be created.");
            LoggingManager.Flush();
            Environment.Exit(2);
        }

        private static void LoadJSON(string statisticsFile) {
            using (StreamReader jsonFile = File.OpenText(statisticsFile)) {
                JsonSerializer jsonSerializer = new JsonSerializer {
                    DateTimeZoneHandling = DateTimeZoneHandling.Local
                };

                Statistics = jsonSerializer.Deserialize(jsonFile, typeof(Statistics)) as Statistics;

                if (Statistics == null) { throw new ArgumentNullException("The stats was null after deserialization."); }
            }
        }

        /// <summary>
        /// Save statistics to the disk at <see cref="Constants.StatisticsFile" />.
        /// </summary>
        public static void Save(bool dirtyCheck = true) {
            if (dirtyCheck) {
                if (!Statistics.IsDirty) { return; }
            }

            string tempFile = $"{Constants.StatisticsFile}.temp";

            try {
                using (StreamWriter streamWriter = new StreamWriter(tempFile))
                using (JsonWriter jsonWriter = new JsonTextWriter(streamWriter)) {
                    DefaultContractResolver contractResolver = new DefaultContractResolver {
                        NamingStrategy = new CamelCaseNamingStrategy()
                    };

                    JsonSerializer jsonSerializer = new JsonSerializer() {
                        ContractResolver = contractResolver,
                        DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                        NullValueHandling = NullValueHandling.Include,
                        Formatting = Formatting.Indented
                    };

                    jsonSerializer.Serialize(jsonWriter, Statistics, typeof(Statistics));
                }

                if (File.Exists(Constants.StatisticsFile)) {
                    File.Copy(Constants.StatisticsFile, Path.ChangeExtension(tempFile, "previous"), true);
                }
                File.Copy(tempFile, Constants.StatisticsFile, true);
                File.Delete(tempFile);

                LoggingManager.Log.Info("Statistics saved.");
            } catch (Exception ex) {
                LoggingManager.Log.Error(ex);
            }

            Statistics.MarkClean();
        }

        /// <summary>
        /// Force default statistics and save to the disk at <see cref="Constants.StatisticsFile" />.
        /// </summary>
        public static void SaveDefault() {
            Statistics = new Statistics();
            Save(false);
        }

    }

}
