using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace HeadNonSub.Entities.Discord.ServerMap {

    public class Role {

        [JsonPropertyName("id")]
        public ulong Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("created")]
        public DateTime Created { get; set; }

        /// <summary>
        /// Hexadecimal color string including the #.
        /// </summary>
        [JsonPropertyName("color")]
        public string Color { get; set; }

        [JsonPropertyName("mentionable")]
        public bool Mentionable { get; set; }

        /// <summary>
        /// Members will appear in a separate section on the user list.
        /// </summary>
        [JsonPropertyName("hoisted")]
        public bool Hoisted { get; set; }

        [JsonPropertyName("permissions")]
        public List<string> Permissions { get; set; } = new List<string>();

        public bool ShouldSerializePermissions() => Permissions.Count > 0;

    }

}
