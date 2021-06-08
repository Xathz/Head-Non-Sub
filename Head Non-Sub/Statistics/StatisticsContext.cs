using System;
using HeadNonSub.Settings;
using HeadNonSub.Statistics.Tables;
using Microsoft.EntityFrameworkCore;

namespace HeadNonSub.Statistics {

    public class StatisticsContext : DbContext {

        public DbSet<Command> Commands { get; set; }

        public DbSet<UserChange> UserChanges { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
            try {
                string connectionString = $"Server={SettingsManager.Configuration.MariaDBHost};" +
                    $"Database={SettingsManager.Configuration.MariaDBDatabase};" +
                    $"Uid={SettingsManager.Configuration.MariaDBUsername};" +
                    $"Pwd={SettingsManager.Configuration.MariaDBPassword};";

                optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));

                optionsBuilder.UseLoggerFactory(LoggingManager.DatabaseFactory);
            } catch (Exception ex) {
                LoggingManager.Log.Error(ex);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            try {
                modelBuilder.Entity<Command>().ToTable("commands");
                modelBuilder.Entity<Command>().HasKey(x => x.Id);

                modelBuilder.Entity<UserChange>().ToTable("user_changes");
                modelBuilder.Entity<UserChange>().HasKey(x => x.Id);
                modelBuilder.Entity<UserChange>().HasIndex(x => x.UserId);
            } catch (Exception ex) {
                LoggingManager.Log.Error(ex);
            }
        }

    }

}
