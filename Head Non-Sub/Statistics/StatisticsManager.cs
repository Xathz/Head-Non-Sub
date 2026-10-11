using HeadNonSub.Extensions;

namespace HeadNonSub.Statistics {

    public static partial class StatisticsManager {

        public static void Load() {
            using StatisticsContext statistics = new StatisticsContext();

            if (statistics.Database.CanConnect()) {
                statistics.EnsureTablesCreated();
            } else {
                LoggingManager.Log.Error("Can connect check failed");
            }
        }

    }

}
