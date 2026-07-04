using Microsoft.Extensions.Options;

namespace HeadNonSub.Settings {

    /// <summary>
    /// Implementation of IConfigurationService using IOptions pattern.
    /// </summary>
    public class ConfigurationService : IConfigurationService {

        private readonly IOptions<Configuration> _Options;

        public ConfigurationService(IOptions<Configuration> options) {
            _Options = options;
        }

        /// <inheritdoc />
        public IOptions<Configuration> Options => _Options;

        /// <inheritdoc />
        public Configuration Value => _Options.Value;

    }

}
