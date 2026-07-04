using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace HeadNonSub.Entities.Discord.ServerMap {

    public class Channel {

        [JsonPropertyName("id")]
        public ulong Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("created")]
        public DateTime Created { get; set; }

        [JsonPropertyName("type")]
        public ChannelType Type { get; set; }

        [JsonPropertyName("topic")]
        public string Topic { get; set; }

        [JsonPropertyName("nsfw")]
        public bool? NSFW { get; set; }

        [JsonPropertyName("userLimit")]
        public int? UserLimit { get; set; }

        [JsonPropertyName("bitrate")]
        public int? Bitrate { get; set; }

        [JsonPropertyName("permissionOverwrites")]
        public List<PermissionOverwrite> PermissionOverwrites { get; set; } = new List<PermissionOverwrite>();

        public bool ShouldSerializePermissionOverwrites() => PermissionOverwrites.Count > 0;

    }

}
