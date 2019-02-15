using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace HeadNonSub.Entities.Discord.ServerMap {

    public class Channel {

        [JsonProperty("id")]
        public ulong Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("created")]
        public DateTime Created { get; set; }

        [JsonProperty("type")]
        public ChannelType Type { get; set; }

        [JsonProperty("topic")]
        public string Topic { get; set; }

        [JsonProperty("nsfw")]
        public bool? NSFW { get; set; }

        [JsonProperty("userLimit")]
        public int? UserLimit { get; set; }

        [JsonProperty("bitrate")]
        public int? Bitrate { get; set; }

        [JsonProperty("permissionOverwrites")]
        public List<PermissionOverwrite> PermissionOverwrites { get; set; } = new List<PermissionOverwrite>();

        public bool ShouldSerializePermissionOverwrites() => PermissionOverwrites.Count > 0;

    }

}
