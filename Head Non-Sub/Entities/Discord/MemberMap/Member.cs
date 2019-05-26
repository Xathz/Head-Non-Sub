using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace HeadNonSub.Entities.Discord.MemberMap {

    public class Member {

        [JsonProperty("id")]
        public ulong Id { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("nickname")]
        public string Nickname { get; set; }

        [JsonProperty("created")]
        public DateTime Created { get; set; }

        [JsonProperty("joined")]
        public DateTime? Joined { get; set; }

        [JsonProperty("avatarUrl")]
        public string AvatarUrl { get; set; }

        [JsonProperty("roles")]
        public List<Role> Roles { get; set; } = new List<Role>();

        public bool ShouldSerializeNickname() => !string.IsNullOrEmpty(Nickname);

        public bool ShouldSerializeRoles() => Roles.Count > 0;

    }

}
