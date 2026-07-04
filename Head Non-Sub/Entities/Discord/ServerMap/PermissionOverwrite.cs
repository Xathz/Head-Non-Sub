using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace HeadNonSub.Entities.Discord.ServerMap {

    public class PermissionOverwrite {

        [JsonPropertyName("target")]
        public PermissionTarget Target { get; set; }

        [JsonPropertyName("id")]
        public ulong Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("permissions")]
        public Dictionary<string, PermissionValue> Permissions { get; set; } = new Dictionary<string, PermissionValue>();

        public bool ShouldSerializePermissions() => Permissions.Count > 0;

    }

}
