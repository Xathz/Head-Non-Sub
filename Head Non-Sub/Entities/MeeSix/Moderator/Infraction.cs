using System;
using Newtonsoft.Json;

namespace HeadNonSub.Entities.MeeSix.Moderator {

    public class Infraction {

        private DateTime? _CreatedAtDateTime;

        [JsonProperty("author_id")]
        public string AuthorId { get; set; }

        [JsonProperty("created_at")]
        public long CreatedAt { get; set; }

        [JsonProperty("guild_id")]
        public string GuildId { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("message")]
        public Message Message { get; set; }

        [JsonProperty("reason")]
        public string Reason { get; set; }

        [JsonProperty("user_id")]
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
