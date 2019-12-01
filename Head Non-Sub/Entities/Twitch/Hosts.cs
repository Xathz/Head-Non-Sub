using System.Collections.Generic;
using Newtonsoft.Json;

namespace HeadNonSub.Entities.Twitch {

    public class HostsResponse {

        [JsonProperty("hosts")]
        public List<Host> Hosts { get; set; }

    }

    public class Host {

        [JsonProperty("host_id")]
        public int HostId { get; set; }

        [JsonProperty("host_login")]
        public string HostLogin { get; set; }

        [JsonProperty("host_display_name")]
        public string HostDisplayName { get; set; }

        [JsonProperty("target_id")]
        public int TargetId { get; set; }

        [JsonProperty("target_login")]
        public string TargetLogin { get; set; }

        [JsonProperty("target_display_name")]
        public string TargetDisplayName { get; set; }

    }

}
