using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace HeadNonSub.Entities.Discord.ServerMap {

    [JsonConverter(typeof(StringEnumConverter))]
    public enum ChannelType {
        Text,
        Voice
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum PermissionTarget {
        Role,
        User
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum PermissionValue {
        Allow,
        Deny,
        Inherit
    }

}
