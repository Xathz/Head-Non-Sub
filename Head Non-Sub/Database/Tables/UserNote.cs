using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using HeadNonSub.Entities.Database.UserNote;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

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
            x => JsonSerializer.Serialize(x, new JsonSerializerOptions { DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull }),
            x => JsonSerializer.Deserialize<List<Note>>(x, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }));

    }

}
