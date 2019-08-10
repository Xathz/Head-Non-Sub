using System;
using HeadNonSub.Database.Tables;
using HeadNonSub.Settings;
using Microsoft.EntityFrameworkCore;

namespace HeadNonSub.Database {

    public class DatabaseContext : DbContext {

        public DbSet<ActiveStream> ActiveStreams { get; set; }

        public DbSet<Cooldown> Cooldowns { get; set; }

        public DbSet<UserNote> UserNotes { get; set; }

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
                // Active streams
                modelBuilder.Entity<ActiveStream>().ToTable("active_streams");
                modelBuilder.Entity<ActiveStream>().HasKey(x => x.Username);

                // Cooldowns
                modelBuilder.Entity<Cooldown>().ToTable("cooldowns");
                modelBuilder.Entity<Cooldown>().HasKey(x => new { x.ServerId, x.UserId, x.Command });
                modelBuilder.Entity<Cooldown>().HasIndex(x => x.ServerId);
                modelBuilder.Entity<Cooldown>().HasIndex(x => x.UserId);

                // User notes
                modelBuilder.Entity<UserNote>().ToTable("user_notes");
                modelBuilder.Entity<UserNote>().HasKey(x => new { x.ServerId, x.UserId });

                modelBuilder.ApplyConfiguration(new UserNoteConfiguration());

            } catch (Exception ex) {
                LoggingManager.Log.Error(ex);
            }
        }

    }

}
