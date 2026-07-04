using System;
using HeadNonSub.Settings;
using HeadNonSub.Statistics.Tables;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace HeadNonSub.Statistics {

    public class StatisticsContext : DbContext {

        private Configuration _Configuration;

        public DbSet<Command> Commands { get; set; }

        public DbSet<UserChange> UserChanges { get; set; }

        public StatisticsContext(IOptions<Configuration> configurationOptions = null) {
            _Configuration = configurationOptions?.Value ?? SettingsManager.Configuration;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
            try {
                string connectionString = $"Server={_Configuration.MariaDBHost};" +
                    $"Database={_Configuration.MariaDBDatabase};" +
                    $"Uid={_Configuration.MariaDBUsername};" +
                    $"Pwd={_Configuration.MariaDBPassword};";

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
