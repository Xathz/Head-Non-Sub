using System;
using Newtonsoft.Json;

namespace HeadNonSub.Entities.Database.UserNote {

    public class Note {

        [JsonProperty("id")]
        public string Id { get; protected set; } = Guid.NewGuid().ToString("N").Substring(0, 12);

        [JsonProperty("datetime")]
        public DateTime DateTime { get; set; }

        [JsonProperty("user_id")]
        public ulong UserId { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }

    }

}
