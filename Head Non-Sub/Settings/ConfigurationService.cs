using Microsoft.Extensions.Options;

namespace HeadNonSub.Settings {

    /// <summary>
    /// Implementation of IConfigurationService using IOptions pattern.
    /// </summary>
    public class ConfigurationService : IConfigurationService {

        public ConfigurationService(IOptions<Configuration> options) => Options = options;

        /// <inheritdoc />
        public IOptions<Configuration> Options { get; }

        /// <inheritdoc />
        public Configuration Value => Options.Value;

    }

}
