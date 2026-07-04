using System;
using System.Text.Json.Serialization;

namespace HeadNonSub.Entities.MeeSix.Moderator {

    public class Infraction {

        private DateTime? _CreatedAtDateTime;

        [JsonPropertyName("author_id")]
        public string AuthorId { get; set; }

        [JsonPropertyName("created_at")]
        public long CreatedAt { get; set; }

        [JsonPropertyName("guild_id")]
        public string GuildId { get; set; }

        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("message")]
        public Message Message { get; set; }

        [JsonPropertyName("reason")]
        public string Reason { get; set; }

        [JsonPropertyName("user_id")]
        public string UserId { get; set; }

        public DateTime CreatedAtDateTime {
            get {
                if (_CreatedAtDateTime.HasValue) {
                    return _CreatedAtDateTime.Value;
                } else {
                    _CreatedAtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(CreatedAt);
                    return _CreatedAtDateTime.Value;
                }
            }
        }

    }

}
