using System;
using System.Text.Json.Serialization;

namespace HeadNonSub.Entities.Database.UserNote {

    public class Note {

        [JsonPropertyName("id")]
        public string Id { get; protected set; } = Guid.NewGuid().ToString("N").Substring(0, 12);

        [JsonPropertyName("datetime")]
        public DateTime DateTime { get; set; }

        [JsonPropertyName("user_id")]
        public ulong UserId { get; set; }

        [JsonPropertyName("text")]
        public string Text { get; set; }

    }

}
