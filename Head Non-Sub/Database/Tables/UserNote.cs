using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using HeadNonSub.Entities.Database.UserNote;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Newtonsoft.Json;

namespace HeadNonSub.Database.Tables {

    public class UserNote {

        [Column("server_id")]
        public ulong ServerId { get; set; }

        [Column("user_id")]
        public ulong UserId { get; set; }

        [Column("notes")]
        public List<Note> Notes { get; set; }

    }

    public class UserNoteConfiguration : IEntityTypeConfiguration<UserNote> {

        public void Configure(EntityTypeBuilder<UserNote> builder) => builder.Property(x => x.Notes).HasConversion(
            x => JsonConvert.SerializeObject(x, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }),
            x => JsonConvert.DeserializeObject<List<Note>>(x, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));

    }

}
