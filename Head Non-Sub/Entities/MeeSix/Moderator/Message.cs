using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace HeadNonSub.Entities.MeeSix.Moderator {

    public class Message {

        [JsonPropertyName("author")]
        public User Author { get; set; }

        [JsonPropertyName("channel_id")]
        public string ChannelId { get; set; }

        [JsonPropertyName("content")]
        public string Content { get; set; }

        [JsonPropertyName("edited_timestamp")]
        public DateTime? EditedTimestamp { get; set; }

        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("mention_everyone")]
        public bool MentionEveryone { get; set; }

        [JsonPropertyName("mentions")]
        public List<User> Mentions { get; set; }

        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; }

        [JsonPropertyName("type")]
        public int Type { get; set; }

    }

}
