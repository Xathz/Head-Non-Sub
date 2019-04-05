using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace HeadNonSub.Database.Tables {

    public class ActiveStream {

        [Column("username")]
        public string Username { get; set; }

        [Column("started_at")]
        public DateTime StartedAt { get; set; }

    }

}
