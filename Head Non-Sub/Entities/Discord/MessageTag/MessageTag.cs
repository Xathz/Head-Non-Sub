namespace HeadNonSub.Entities.Discord.MessageTag {

    public class MessageTag {

        public MessageTag(TagType tagType, ulong id, int index, int length) {
            TagType = tagType;
            Id = id;
            Index = index;
            Length = length;
        }

        public TagType TagType { get; private set; }

        public ulong Id { get; private set; }

        public int Index { get; private set; }

        public int Length { get; private set; }

        /// <summary>
        /// Tag id formatted based on <see cref="TagType"/>.
        /// </summary>
        public override string ToString() {
            switch (TagType) {
                case TagType.User:
                    return $"<@{Id}>";
                case TagType.Role:
                    return $"<@&{Id}>";
                case TagType.Channel:
                    return $"<#{Id}>";
                case TagType.Everyone:
                    return "@everyone";
                case TagType.Here:
                    return "@here";
                default:
                    return Id.ToString();
            }
        }

    }

    public enum TagType {
        User,
        Role,
        Channel,
        Everyone,
        Here
    }

}
