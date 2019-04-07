using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace HeadNonSub.Database.Tables {

    public class Stalker {

        [Column("server_id")]
        public ulong ServerId { get; set; }

        [Column("user_id")]
        public ulong UserId { get; set; }

        [Column("stalking_user_id")]
        public ulong StalkingUserId { get; set; }

        [Column("datetime")]
        public DateTime DateTime { get; set; }

    }

}
