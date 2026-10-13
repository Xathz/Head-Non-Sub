using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HeadNonSub.Settings {

    public static class SettingsManager {

        /// <summary>
        /// Configuration settings.
        /// </summary>
        public static Configuration Configuration = new Configuration();

        private static readonly JsonSerializerOptions _LoadOptions = new JsonSerializerOptions {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        private static readonly JsonSerializerOptions _SaveOptions = new JsonSerializerOptions {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.Never
        };

        /// <summary>
        /// Load settings from the disk at <see cref="Constants.SettingsFile" />.
        /// </summary>
        public static void Load() {
            if (!File.Exists(Constants.SettingsFile)) {
                LoggingManager.Log.Warn($"Settings file was not found at '{Constants.SettingsFile}', creating default one");
                SaveDefault();
            }

            try {
                LoadJSON(Constants.SettingsFile);

                LoggingManager.Log.Info("Settings loaded");
                return;
            } catch (Exception ex) {
                LoggingManager.Log.Error(ex);
            }

            Shutdown.FailFast($"Can not load settings file '{Constants.SettingsFile}', check it or delete it so a new one can be created");
        }

        private static void LoadJSON(string settingsFile) {
            Configuration = JsonSerializer.Deserialize<Configuration>(File.ReadAllText(settingsFile), _LoadOptions)
                ?? throw new JsonException("The configuration was null after deserialization");
        }

        /// <summary>
        /// Save settings to the disk at <see cref="Constants.SettingsFile" />.
        /// </summary>
        public static void Save() {
            string tempFile = $"{Constants.SettingsFile}.temp";

            try {
                string json = JsonSerializer.Serialize(Configuration, _SaveOptions);

                File.WriteAllText(tempFile, json);

                if (File.Exists(Constants.SettingsFile)) {
                    File.Copy(Constants.SettingsFile, Path.ChangeExtension(tempFile, "previous"), true);
                }

                File.Copy(tempFile, Constants.SettingsFile, true);
                File.Delete(tempFile);

                LoggingManager.Log.Info("Settings saved");
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
