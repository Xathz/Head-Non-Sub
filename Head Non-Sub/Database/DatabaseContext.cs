using System;
using HeadNonSub.Database.Tables;
using HeadNonSub.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace HeadNonSub.Database {

    public class DatabaseContext : DbContext {

        private readonly Configuration _Configuration;

        public DbSet<Cooldown> Cooldowns { get; set; }

        /// <summary>
        /// Constructor with optional configuration injection.
        /// </summary>
        public DatabaseContext(IOptions<Configuration> configurationOptions = null) => _Configuration = configurationOptions?.Value ?? SettingsManager.Configuration;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
            // Let connection errors through, catching them here left the context without a provider
            string connectionString = DatabaseConnection.GetConnectionString(_Configuration);

            optionsBuilder.UseMySql(connectionString, DatabaseConnection.GetServerVersion(connectionString));
            optionsBuilder.UseLoggerFactory(LoggingManager.DatabaseFactory);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            try {
                // Cooldowns
                modelBuilder.Entity<Cooldown>().ToTable("cooldowns");
                modelBuilder.Entity<Cooldown>().HasKey(x => new { x.ServerId, x.UserId, x.Command });
                modelBuilder.Entity<Cooldown>().HasIndex(x => x.ServerId);
                modelBuilder.Entity<Cooldown>().HasIndex(x => x.UserId);

            } catch (Exception ex) {
                LoggingManager.Log.Error(ex);
            }
        }

    }

}
