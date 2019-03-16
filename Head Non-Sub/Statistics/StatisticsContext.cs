using System;
using HeadNonSub.Settings;
using HeadNonSub.Statistics.Tables;
using Microsoft.EntityFrameworkCore;

namespace HeadNonSub.Statistics {

    public class StatisticsContext : DbContext {

        public DbSet<Command> Commands { get; set; }

        public StatisticsContext() => Database.EnsureCreated();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
            try {
                LoggingManager.Log.Info("Configuring database");

                optionsBuilder.UseMySql($"Server={SettingsManager.Configuration.MariaDBHost};" +
                    $"Database={SettingsManager.Configuration.MariaDBDatabase};" +
                    $"Uid={SettingsManager.Configuration.MariaDBUsername};" +
                    $"Pwd={SettingsManager.Configuration.MariaDBPassword};");

            } catch (Exception ex) {
                LoggingManager.Log.Error(ex);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            try {
                LoggingManager.Log.Info("Creating database model");

                modelBuilder.Entity<Command>().ToTable("commands");
                modelBuilder.Entity<Command>().HasKey(x => x.Id);
                modelBuilder.Entity<Command>().HasIndex(x => x.CommandName);
            } catch (Exception ex) {
                LoggingManager.Log.Error(ex);
            }
        }

    }

}
