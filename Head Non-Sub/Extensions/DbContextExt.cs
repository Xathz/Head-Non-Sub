using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace HeadNonSub.Extensions {

    public static class DbContextExt {

        /// <summary>
        /// Create the tables for a context if none of them exist yet. Unlike EnsureCreated this works when multiple contexts share a database.
        /// </summary>
        /// <param name="context">Database context to create the tables for.</param>
        public static void EnsureTablesCreated(this DbContext context) {
            string name = context.GetType().Name;

            List<string> existing = context.Database.SqlQueryRaw<string>("SELECT table_name AS Value FROM information_schema.tables WHERE table_schema = DATABASE()").ToList();
            List<string> tables = context.Model.GetEntityTypes().Select(x => x.GetTableName()).Where(x => x != null).Distinct().ToList();
            List<string> missing = tables.Where(x => !existing.Contains(x, StringComparer.OrdinalIgnoreCase)).ToList();

            if (missing.Count == 0) {
                LoggingManager.Log.Info($"{name} tables exist, connected");
            } else if (missing.Count == tables.Count) {
                context.GetService<IRelationalDatabaseCreator>().CreateTables();
                LoggingManager.Log.Info($"{name} tables created: {string.Join(", ", missing)}");
            } else {
                LoggingManager.Log.Error($"{name} is missing tables, create them with the scripts in the Database folder: {string.Join(", ", missing)}");
            }
        }

    }

}
