using System;
using HeadNonSub.Database.Tables;
using HeadNonSub.Settings;
using Microsoft.EntityFrameworkCore;

namespace HeadNonSub.Database {

    public class DatabaseContext : DbContext {

        public DbSet<UserNote> UserNotes { get; set; }
        public DbSet<DynamicCommand> DynamicCommands { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
            try {
                optionsBuilder.UseMySql($"Server={SettingsManager.Configuration.MariaDBHost};" +
                    $"Database={SettingsManager.Configuration.MariaDBDatabase};" +
                    $"Uid={SettingsManager.Configuration.MariaDBUsername};" +
                    $"Pwd={SettingsManager.Configuration.MariaDBPassword};");

                optionsBuilder.UseLoggerFactory(LoggingManager.DatabaseFactory);
            } catch (Exception ex) {
                LoggingManager.Log.Error(ex);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            try {
                modelBuilder.Entity<UserNote>().ToTable("user_notes");
                modelBuilder.Entity<UserNote>().HasKey(x => new { x.ServerId, x.UserId });

                modelBuilder.ApplyConfiguration(new UserNoteConfiguration());

                modelBuilder.Entity<DynamicCommand>().ToTable("dynamic_commands");
                modelBuilder.Entity<DynamicCommand>().HasKey(x => x.OwnerId);
            } catch (Exception ex) {
                LoggingManager.Log.Error(ex);
            }
        }

    }

}
