using HeadNonSub.Settings;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;

namespace HeadNonSub.Database {

    /// <summary>
    /// Connection details shared by the database contexts.
    /// </summary>
    public static class DatabaseConnection {

        private static readonly object _Lock = new object();
        private static ServerVersion _ServerVersion;

        /// <summary>
        /// Build the connection string from the settings.
        /// </summary>
        /// <param name="configuration">Settings with the MariaDB connection details.</param>
        public static string GetConnectionString(Configuration configuration) => new MySqlConnectionStringBuilder {
            Server = configuration.MariaDBHost,
            Database = configuration.MariaDBDatabase,
            UserID = configuration.MariaDBUsername,
            Password = configuration.MariaDBPassword
        }.ConnectionString;

        /// <summary>
        /// Detect the server version once instead of on every new context. Throws if the server can not be reached.
        /// </summary>
        /// <param name="connectionString">Connection string to detect the version with.</param>
        public static ServerVersion GetServerVersion(string connectionString) {
            lock (_Lock) {
                // Only cached once detected, so a failed attempt is retried
                _ServerVersion ??= ServerVersion.AutoDetect(connectionString);
                return _ServerVersion;
            }
        }

    }

}
