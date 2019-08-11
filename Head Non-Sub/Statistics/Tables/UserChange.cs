using System;
using System.ComponentModel.DataAnnotations.Schema;
using static HeadNonSub.Statistics.StatisticsManager;

namespace HeadNonSub.Statistics.Tables {

    public class UserChange {

        [Column("id")]
        public uint Id { get; set; }

        [Column("datetime")]
        public DateTime DateTime { get; set; }

        [Column("server_id")]
        public ulong? ServerId { get; set; }

        [Column("user_id")]
        public ulong UserId { get; set; }

        [Column("change_type")]
        public NameChangeType ChangeType { get; set; }

        [Column("old_user_name")]
        public string OldUsername { get; set; }

        [Column("new_user_name")]
        public string NewUsername { get; set; }

        [Column("old_user_name_discriminator")]
        public string OldUsernameDiscriminator { get; set; }

        [Column("new_user_name_discriminator")]
        public string NewUsernameDiscriminator { get; set; }

        [Column("old_user_display")]
        public string OldUserDisplay { get; set; }

        [Column("new_user_display")]
        public string NewUserDisplay { get; set; }

        [Column("backblaze_avatar_bucket")]
        public string BackblazeAvatarBucket { get; set; }

        [Column("backblaze_avatar_filename")]
        public string BackblazeAvatarFilename { get; set; }

        [Column("backblaze_avatar_url")]
        public string BackblazeAvatarUrl { get; set; }

    }

}
