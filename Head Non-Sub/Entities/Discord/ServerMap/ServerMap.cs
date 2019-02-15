using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace HeadNonSub.Entities.Discord.ServerMap {

    public class ServerMap {

        [JsonProperty("id")]
        public ulong Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("created")]
        public DateTime Created { get; set; }

        [JsonProperty("owner")]
        public User Owner { get; set; }

        [JsonProperty("totalMembers")]
        public int TotalMembers { get; set; }

        [JsonProperty("voiceRegion")]
        public string VoiceRegion { get; set; }

        [JsonProperty("verificationLevel")]
        public string VerificationLevel { get; set; }

        [JsonProperty("iconUrl")]
        public string IconUrl { get; set; }

        [JsonProperty("roles")]
        public List<Role> Roles { get; set; } = new List<Role>();

        [JsonProperty("categories")]
        public List<Category> Categories { get; set; } = new List<Category>();

        [JsonProperty("categorylessChannels")]
        public List<Channel> CategorylessChannels { get; set; } = new List<Channel>();

        [JsonProperty("emotes")]
        public List<Emote> Emotes { get; set; } = new List<Emote>();

        public bool ShouldSerializeRoles() => Roles.Count > 0;

        public bool ShouldSerializeCategories() => Categories.Count > 0;

        public bool ShouldSerializeCategorylessChannels() => CategorylessChannels.Count > 0;

        public bool ShouldSerializeEmotes() => Emotes.Count > 0;

    }

}
