using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using System.Text.Json.Serialization;
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

        private static readonly JsonSerializerOptions _SerializeOptions = new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };
        private static readonly JsonSerializerOptions _DeserializeOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        public void Configure(EntityTypeBuilder<UserNote> builder) => builder.Property(x => x.Notes).HasConversion(
            x => JsonSerializer.Serialize(x, _SerializeOptions),
            x => JsonSerializer.Deserialize<List<Note>>(x, _DeserializeOptions));

    }

}
