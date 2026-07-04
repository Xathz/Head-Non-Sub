using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace HeadNonSub.Entities.Discord.ServerMap {

    public class Category {

        [JsonPropertyName("id")]
        public ulong Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("created")]
        public DateTime Created { get; set; }

        [JsonPropertyName("channels")]
        public List<Channel> Channels { get; set; } = new List<Channel>();

        [JsonPropertyName("permissionOverwrites")]
        public List<PermissionOverwrite> PermissionOverwrites { get; set; } = new List<PermissionOverwrite>();

        public bool ShouldSerializeChannels() => Channels.Count > 0;

        public bool ShouldSerializePermissionOverwrites() => PermissionOverwrites.Count > 0;

    }

}
