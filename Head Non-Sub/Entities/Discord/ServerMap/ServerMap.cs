using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace HeadNonSub.Entities.Discord.ServerMap {

    public class ServerMap {

        [JsonPropertyName("id")]
        public ulong Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("created")]
        public DateTime Created { get; set; }

        [JsonPropertyName("owner")]
        public User Owner { get; set; }

        [JsonPropertyName("totalMembers")]
        public int TotalMembers { get; set; }

        [JsonPropertyName("voiceRegion")]
        public string VoiceRegion { get; set; }

        [JsonPropertyName("verificationLevel")]
        public string VerificationLevel { get; set; }

        [JsonPropertyName("iconUrl")]
        public string IconUrl { get; set; }

        [JsonPropertyName("roles")]
        public List<Role> Roles { get; set; } = new List<Role>();

        [JsonPropertyName("categories")]
        public List<Category> Categories { get; set; } = new List<Category>();

        [JsonPropertyName("categorylessChannels")]
        public List<Channel> CategorylessChannels { get; set; } = new List<Channel>();

        [JsonPropertyName("emotes")]
        public List<Emote> Emotes { get; set; } = new List<Emote>();

        public bool ShouldSerializeRoles() => Roles.Count > 0;

        public bool ShouldSerializeCategories() => Categories.Count > 0;

        public bool ShouldSerializeCategorylessChannels() => CategorylessChannels.Count > 0;

        public bool ShouldSerializeEmotes() => Emotes.Count > 0;

    }

}
