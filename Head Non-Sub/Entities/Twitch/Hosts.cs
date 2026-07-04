using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace HeadNonSub.Entities.Twitch {

    public class HostsResponse {

        [JsonPropertyName("hosts")]
        public List<Host> Hosts { get; set; }

    }

    public class Host {

        [JsonPropertyName("host_id")]
        public int HostId { get; set; }

        [JsonPropertyName("host_login")]
        public string HostLogin { get; set; }

        [JsonPropertyName("host_display_name")]
        public string HostDisplayName { get; set; }

        [JsonPropertyName("target_id")]
        public int TargetId { get; set; }

        [JsonPropertyName("target_login")]
        public string TargetLogin { get; set; }

        [JsonPropertyName("target_display_name")]
        public string TargetDisplayName { get; set; }

    }

}
