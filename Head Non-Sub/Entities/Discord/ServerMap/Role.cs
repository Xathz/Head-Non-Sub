using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace HeadNonSub.Entities.Discord.ServerMap {

    public class Role {

        [JsonProperty("id")]
        public ulong Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("created")]
        public DateTime Created { get; set; }

        /// <summary>
        /// Hexadecimal color string including the #.
        /// </summary>
        [JsonProperty("color")]
        public string Color { get; set; }

        [JsonProperty("mentionable")]
        public bool Mentionable { get; set; }

        /// <summary>
        /// Members will appear in a separate section on the user list.
        /// </summary>
        [JsonProperty("hoisted")]
        public bool Hoisted { get; set; }

        [JsonProperty("permissions")]
        public List<string> Permissions { get; set; } = new List<string>();

        public bool ShouldSerializePermissions() => Permissions.Count > 0;

    }

}
