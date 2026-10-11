using System;
using HeadNonSub.Database;
using HeadNonSub.Settings;
using HeadNonSub.Statistics.Tables;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace HeadNonSub.Statistics {

    public class StatisticsContext : DbContext {

        private readonly Configuration _Configuration;

        public DbSet<Command> Commands { get; set; }

        public StatisticsContext(IOptions<Configuration> configurationOptions = null) => _Configuration = configurationOptions?.Value ?? SettingsManager.Configuration;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
            // Let connection errors through, catching them here left the context without a provider
            string connectionString = DatabaseConnection.GetConnectionString(_Configuration);

            optionsBuilder.UseMySql(connectionString, DatabaseConnection.GetServerVersion(connectionString));
            optionsBuilder.UseLoggerFactory(LoggingManager.DatabaseFactory);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            try {
                modelBuilder.Entity<Command>().ToTable("commands");
                modelBuilder.Entity<Command>().HasKey(x => x.Id);
            } catch (Exception ex) {
                LoggingManager.Log.Error(ex);
            }
        }

    }

}
