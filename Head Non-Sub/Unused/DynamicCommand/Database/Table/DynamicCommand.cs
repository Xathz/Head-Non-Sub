using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace HeadNonSub.Database.Tables {

    public class DynamicCommand {

        [Column("owner_id")]
        public ulong OwnerId { get; set; }

        [Column("datetime")]
        public DateTime DateTime { get; set; }

        [Column("command")]
        public string Command { get; set; }

        [Column("text")]
        public string Text { get; set; }

    }

}
