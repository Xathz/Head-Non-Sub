using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace HeadNonSub.Entities.Discord.MemberMap {

    public class Member {

        [JsonPropertyName("id")]
        public ulong Id { get; set; }

        [JsonPropertyName("username")]
        public string Username { get; set; }

        [JsonPropertyName("nickname")]
        public string Nickname { get; set; }

        [JsonPropertyName("created")]
        public DateTime Created { get; set; }

        [JsonPropertyName("joined")]
        public DateTime? Joined { get; set; }

        [JsonPropertyName("avatarUrl")]
        public string AvatarUrl { get; set; }

        [JsonPropertyName("roles")]
        public List<Role> Roles { get; set; } = new List<Role>();

        public bool ShouldSerializeNickname() => !string.IsNullOrEmpty(Nickname);

        public bool ShouldSerializeRoles() => Roles.Count > 0;

    }

}
