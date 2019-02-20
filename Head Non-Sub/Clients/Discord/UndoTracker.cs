using System.Linq;
using System.Collections.Generic;
using System;

namespace HeadNonSub.Clients.Discord {

    public static class UndoTracker {

        private static List<(DateTime dateTime, ulong server, ulong channel, ulong userId, ulong messageId, ulong replyId)> _SentMessages = 
                    new List<(DateTime dateTime, ulong server, ulong channel, ulong userId, ulong messageId, ulong replyId)>();

        /// <summary>
        /// Track a message.
        /// </summary>
        /// <param name="server">Server (guild) id.</param>
        /// <param name="channel">Channel id.</param>
        /// <param name="userId">Invoking user id.</param>
        /// <param name="replyId">Message reply id.</param>
        public static void Track(ulong server, ulong channel, ulong userId, ulong messageId, ulong replyId) {
            _SentMessages.Add((DateTime.Now, server, channel, userId, messageId, replyId));

            // Remove oldest
            if (_SentMessages.Count > 250) {
                _SentMessages.RemoveAt(0);
            }
        }

        /// <summary>
        /// Untrack a message by a message id or reply id.
        /// </summary>
        public static void Untrack(ulong id) => _SentMessages.RemoveAll(x => x.replyId == id || x.messageId == id);

        /// <summary>
        /// Get the most recent message id by user.
        /// </summary>
        public static ulong? MostRecentMessage(ulong server, ulong channel, ulong userId) =>
            _SentMessages.OrderByDescending(x => x.dateTime).Where(x => x.server == server & x.channel == channel & x.userId == userId).Select(x => x.messageId).FirstOrDefault();

        /// <summary>
        /// Get the most recent reply id by user.
        /// </summary>
        public static ulong? MostRecentReply(ulong server, ulong channel, ulong userId) =>
            _SentMessages.OrderByDescending(x => x.dateTime).Where(x => x.server == server & x.channel == channel & x.userId == userId).Select(x => x.replyId).FirstOrDefault();

    }

}
