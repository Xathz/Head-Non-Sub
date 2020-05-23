using System;

namespace HeadNonSub.Statistics {

    public static partial class StatisticsManager {

        public static void Load() {
            using StatisticsContext statistics = new StatisticsContext();

            if (statistics.Database.CanConnect()) {
                if (statistics.Database.EnsureCreated()) {
                    LoggingManager.Log.Info("Database created");
                } else {
                    LoggingManager.Log.Info("Database already exists, connected");
                }
            } else {
                LoggingManager.Log.Error("Can connect check failed");
            }
        }

        [Flags]
        public enum NameChangeType : ushort {
            None = 0,
            Username = 1,
            Discriminator = 2,
            Display = 4,
            Avatar = 8
        }

    }

}
