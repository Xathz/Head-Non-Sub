using System.Collections.Generic;
using Newtonsoft.Json;

namespace HeadNonSub.Entities.Discord.ServerMap {

    public class PermissionOverwrite {

        [JsonProperty("target")]
        public PermissionTarget Target { get; set; }

        [JsonProperty("id")]
        public ulong Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("permissions")]
        public Dictionary<string, PermissionValue> Permissions { get; set; } = new Dictionary<string, PermissionValue>();

        public bool ShouldSerializePermissions() => Permissions.Count > 0;

    }

}
