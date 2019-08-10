using System.Collections.Concurrent;

namespace HeadNonSub.Clients.Discord {

    public class RaveTracker {

        private static ConcurrentDictionary<ulong, Status> _Raves = new ConcurrentDictionary<ulong, Status>();

        /// <summary>
        /// Track a rave.
        /// </summary>
        /// <param name="channel">Channel id.</param>
        public static void Track(ulong channel) => _Raves.TryAdd(channel, Status.Running);

        /// <summary>
        /// Stop a rave.
        /// </summary>
        /// <param name="channel">Channel id.</param>
        public static void Stop(ulong channel) {
            if (_Raves.ContainsKey(channel)) {
                _Raves.TryUpdate(channel, Status.StopRequested, Status.Running);
            }
        }

        /// <summary>
        /// Check if a rave should be stopped.
        /// </summary>
        /// <param name="channel">Channel id.</param>
        public static Status GetStatus(ulong channel) {
            if (_Raves.ContainsKey(channel)) {

                if (_Raves.TryGetValue(channel, out Status status)) {
                    if (status == Status.StopRequested) {
                        _Raves.TryRemove(channel, out Status _);
                    } 
                }

                return status;

            } else {
                return Status.InvalidChannel;
            }
        }

        public enum Status {
            Running,
            StopRequested,
            InvalidChannel
        }

    }

}
