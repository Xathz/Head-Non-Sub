using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace HeadNonSub.Entities.MeeSix.Moderator {

    public class Message {

        [JsonProperty("author")]
        public User Author { get; set; }

        [JsonProperty("channel_id")]
        public string ChannelId { get; set; }

        [JsonProperty("content")]
        public string Content { get; set; }

        [JsonProperty("edited_timestamp")]
        public DateTime? EditedTimestamp { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("mention_everyone")]
        public bool MentionEveryone { get; set; }

        [JsonProperty("mentions")]
        public List<User> Mentions { get; set; }

        [JsonProperty("timestamp")]
        public DateTime Timestamp { get; set; }

        [JsonProperty("type")]
        public int Type { get; set; }

    }

}
