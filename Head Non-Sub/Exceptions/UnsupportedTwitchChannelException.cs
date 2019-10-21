using System;

namespace HeadNonSub.Exceptions {

    public class UnsupportedTwitchChannelException : Exception {

        public UnsupportedTwitchChannelException() { }

        public UnsupportedTwitchChannelException(string message) : base(message) { }

        public UnsupportedTwitchChannelException(string message, Exception innerException) : base(message, innerException) { }

    }

}
