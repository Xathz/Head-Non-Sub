using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace HeadNonSub.Settings {

    public static class SettingsManager {

        /// <summary>
        /// Configuration settings.
        /// </summary>
        public static Configuration Configuration = new Configuration();

        /// <summary>
        /// Load settings from the disk at <see cref="Constants.SettingsFile" />.
        /// </summary>
        public static void Load() {
            if (!File.Exists(Constants.SettingsFile)) {
                LoggingManager.Log.Warn($"Settings file was not found at '{Constants.SettingsFile}', creating default one.");
                SaveDefault();
            }

            try {
                LoadJSON(Constants.SettingsFile);

                LoggingManager.Log.Info("Settings loaded.");
                return;
            } catch (Exception ex) {
                LoggingManager.Log.Error(ex);
            }

            LoggingManager.Log.Fatal($"Can not load settings file '{Constants.SettingsFile}', please check it or delete it so a new one can be created.");
            LoggingManager.Flush();
            Environment.Exit(2);
        }

        private static void LoadJSON(string settingsFile) {
            using (StreamReader jsonFile = File.OpenText(settingsFile)) {
                JsonSerializer jsonSerializer = new JsonSerializer {
                    DateTimeZoneHandling = DateTimeZoneHandling.Local
                };

                Configuration = jsonSerializer.Deserialize(jsonFile, typeof(Configuration)) as Configuration;

                if (Configuration == null) { throw new ArgumentNullException("The configuration was null after deserialization."); }
            }
        }

        /// <summary>
        /// Save settings to the disk at <see cref="Constants.SettingsFile" />.
        /// </summary>
        public static void Save() {
            string tempFile = $"{Constants.SettingsFile}.temp";

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

                    jsonSerializer.Serialize(jsonWriter, Configuration, typeof(Configuration));
                }

                File.Copy(Constants.SettingsFile, Path.ChangeExtension(tempFile, "previous"), true);
                File.Copy(tempFile, Constants.SettingsFile, true);
                File.Delete(tempFile);

                LoggingManager.Log.Info("Settings saved.");
            } catch (Exception ex) {
                LoggingManager.Log.Error(ex);
            }
        }

        /// <summary>
        /// Force default settings and save to the disk at <see cref="Constants.SettingsFile" />.
        /// </summary>
        public static void SaveDefault() {
            Configuration = new Configuration();
            Save();
        }

    }

}
