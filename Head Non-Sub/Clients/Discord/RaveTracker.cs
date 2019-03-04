using System;
using System.Collections.Generic;
using System.Linq;

namespace HeadNonSub.Clients.Discord {

    public class RaveTracker {

        private static List<Rave> _Raves = new List<Rave>();

        /// <summary>
        /// Track a rave.
        /// </summary>
        /// <param name="server">Server (guild) id.</param>
        /// <param name="channel">Channel id.</param>
        public static void Track(ulong server, ulong channel) {
            _Raves.Add(new Rave(DateTime.Now, server, channel, true));
        }

        /// <summary>
        /// Stop a rave.
        /// </summary>
        /// <param name="server">Server (guild) id.</param>
        /// <param name="channel">Channel id.</param>
        public static void Stop(ulong server, ulong channel) {
            _Raves.Where(x => x.Server == server & x.Channel == channel).ToList().ForEach(x => x.Active = false);
        }

        /// <summary>
        /// Check if a rave is to be stopped.
        /// </summary>
        /// <param name="server">Server (guild) id.</param>
        /// <param name="channel">Channel id.</param>
        public static bool IsStopped(ulong server, ulong channel) {
            bool isStopped = _Raves.Where(x => x.Server == server & x.Channel == channel & x.Active == false).OrderByDescending(x => x.DateTime).Any();

            if (isStopped) {
                _Raves.RemoveAt(_Raves.FindIndex(x => x.Server == server & x.Channel == channel & x.Active == false));
            }

            return isStopped;
        }

        private class Rave {

            public Rave(DateTime dateTime, ulong server, ulong channel, bool active) {
                DateTime = dateTime;
                Server = server;
                Channel = channel;
                Active = active;
            }

            public DateTime DateTime { get; protected set; }
            public ulong Server { get; protected set; }
            public ulong Channel { get; protected set; }
            public bool Active { get; set; }
        }

    }

}
