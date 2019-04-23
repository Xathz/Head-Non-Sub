using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace HeadNonSub.Database.Tables {

    public class Cooldown {

        [Column("datetime_offset")]
        public DateTimeOffset DateTimeOffset { get; set; }

        [Column("server_id")]
        public ulong ServerId { get; set; }

        [Column("user_id")]
        public ulong UserId { get; set; }

        [Column("command")]
        public string Command { get; set; }

    }

}
