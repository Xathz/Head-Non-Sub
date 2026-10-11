using HeadNonSub.Extensions;

namespace HeadNonSub.Database {

    public static partial class DatabaseManager {

        public static void Load() {
            using DatabaseContext database = new DatabaseContext();

            if (database.Database.CanConnect()) {
                database.EnsureTablesCreated();
            } else {
                LoggingManager.Log.Error("Can connect check failed");
            }
        }

    }

}
