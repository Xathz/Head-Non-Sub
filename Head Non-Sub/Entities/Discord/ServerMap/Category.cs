using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace HeadNonSub.Entities.Discord.ServerMap {

    public class Category {

        [JsonProperty("id")]
        public ulong Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("created")]
        public DateTime Created { get; set; }

        [JsonProperty("channels")]
        public List<Channel> Channels { get; set; } = new List<Channel>();

        [JsonProperty("permissionOverwrites")]
        public List<PermissionOverwrite> PermissionOverwrites { get; set; } = new List<PermissionOverwrite>();

        public bool ShouldSerializeChannels() => Channels.Count > 0;

        public bool ShouldSerializePermissionOverwrites() => PermissionOverwrites.Count > 0;

    }

}
