using System.Collections.Generic;

namespace HeadNonSub.Settings {

    /// <summary>
    /// Validates configuration settings at startup.
    /// </summary>
    public static class ConfigurationValidator {

        /// <summary>
        /// Validate the configuration and return any errors.
        /// </summary>
        /// <param name="configuration">Configuration to validate.</param>
        /// <returns>List of validation errors, empty if no errors.</returns>
        public static List<string> Validate(Configuration configuration) {
            List<string> errors = new List<string>();

            if (configuration == null) {
                errors.Add("Configuration is null");
                return errors;
            }

            // Discord Token
            if (string.IsNullOrWhiteSpace(configuration.DiscordToken)) {
                errors.Add("DiscordToken is required and cannot be empty");
            }

            // Database Configuration
            if (string.IsNullOrWhiteSpace(configuration.MariaDBHost)) {
                errors.Add("MariaDBHost is required and cannot be empty");
            }

            if (string.IsNullOrWhiteSpace(configuration.MariaDBDatabase)) {
                errors.Add("MariaDBDatabase is required and cannot be empty");
            }

            if (string.IsNullOrWhiteSpace(configuration.MariaDBUsername)) {
                errors.Add("MariaDBUsername is required and cannot be empty");
            }

            if (string.IsNullOrWhiteSpace(configuration.MariaDBPassword)) {
                errors.Add("MariaDBPassword is required and cannot be empty");
            }

            return errors;
        }

    }

}
