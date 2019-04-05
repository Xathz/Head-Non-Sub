namespace HeadNonSub.Database {

    public static partial class DatabaseManager {

        public static void Load() {
            using (DatabaseContext database = new DatabaseContext()) {
                if (database.Database.CanConnect()) {
                    if (database.Database.EnsureCreated()) {
                        LoggingManager.Log.Info("Database created");
                    } else {
                        LoggingManager.Log.Info("Database already exists, connected");
                    }
                } else {
                    LoggingManager.Log.Error("Can connect check failed");
                }
            }
        }

    }

}
