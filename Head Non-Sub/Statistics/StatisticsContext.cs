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
            modelBuilder.Entity<Command>().ToTable("commands");
            modelBuilder.Entity<Command>().HasKey(x => x.Id);

            // Varchar so the index alone can answer the /stats counts, see Database/Tables/commands_v3.sql
            modelBuilder.Entity<Command>().Property(x => x.CommandName).HasMaxLength(255).IsRequired();
            modelBuilder.Entity<Command>().HasIndex(x => new { x.ServerId, x.CommandName }).HasDatabaseName("server_id_command");
        }

    }

}
