using System;
using HeadNonSub.Settings;
using HeadNonSub.Database.Tables;
using Microsoft.EntityFrameworkCore;

namespace HeadNonSub.Database {

    public class DatabaseContext : DbContext {

        public DbSet<UserNote> UserNotes { get; set; }

        public DatabaseContext() {
            try {
                LoggingManager.Log.Info("Connecting to database");

                Database.EnsureCreated();
            } catch (Exception ex) {
                LoggingManager.Log.Error(ex);
            }
        }

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

                modelBuilder.Entity<UserNote>().ToTable("user_notes");
                modelBuilder.Entity<UserNote>().HasKey(x => new { x.ServerId, x.UserId });

                modelBuilder.ApplyConfiguration(new UserNoteConfiguration());

            } catch (Exception ex) {
                LoggingManager.Log.Error(ex);
            }
        }

    }

}
