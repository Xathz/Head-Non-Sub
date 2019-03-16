using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace HeadNonSub.Statistics.Tables {

    public class Command {

        [Column("id")]
        public uint Id { get; set; }

        [Column("datetime")]
        public DateTime DateTime { get; set; }

        [Column("server_id")]
        public ulong ServerId { get; set; }

        [Column("channel_id")]
        public ulong ChannelId { get; set; }

        [Column("user_id")]
        public ulong UserId { get; set; }

        [Column("user_name")]
        public string Username { get; set; }

        [Column("user_display")]
        public string UserDisplay { get; set; }

        [Column("message_id")]
        public ulong MessageId { get; set; }

        [Column("message")]
        public string Message { get; set; }

        [Column("command")]
        public string CommandName { get; set; }

        [Column("parameters")]
        public string Parameters { get; set; }

    }

}
