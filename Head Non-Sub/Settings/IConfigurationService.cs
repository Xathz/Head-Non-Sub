using Microsoft.Extensions.Options;

namespace HeadNonSub.Settings {

    /// <summary>
    /// Service for accessing application configuration.
    /// </summary>
    public interface IConfigurationService {

        /// <summary>
        /// Get the current configuration options.
        /// </summary>
        IOptions<Configuration> Options { get; }

        /// <summary>
        /// Get the underlying configuration value.
        /// </summary>
        Configuration Value { get; }

    }

}
