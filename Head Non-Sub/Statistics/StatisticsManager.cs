using System;
using System.Collections.Generic;
using System.Linq;
using HeadNonSub.Statistics.Tables;

namespace HeadNonSub.Statistics {

    public static class StatisticsManager {

        private static StatisticsContext _Statistics;

        public static void Load() {
            if (_Statistics is null) {
                _Statistics = new StatisticsContext();

                LoggingManager.Log.Info("Connected");
            } else {
                LoggingManager.Log.Info("Loading is already complete");
            }   
        }

        /// <summary>
        /// Insert a command that was ran into the database.
        /// </summary>
        public static void InsertCommand(DateTime dateTime, ulong serverId, ulong channelId, ulong userId, string username, string userDisplay, ulong messageId, string message, string command, string parameters) {
            try {

                Command item = new Command {
                    DateTime = dateTime,
                    ServerId = serverId,
                    ChannelId = channelId,
                    UserId = userId,
                    Username = username,
                    UserDisplay = userDisplay,
                    MessageId = messageId,
                    Message = message,
                    CommandName = command,
                    Parameters = parameters
                };

                _Statistics.Commands.Add(item);

                _Statistics.SaveChangesAsync();

            } catch (Exception ex) {
                LoggingManager.Log.Error(ex);
            }
        }

        /// <summary>
        /// Get the number of times the true command was executed.
        /// </summary>
        public static long TrueCount(ulong serverId) => _Statistics.Commands.Where(x => x.ServerId == serverId & x.CommandName == "ThatsTrue").LongCount();

        /// <summary>
        /// Get the number of times each says command was executed order by count.
        /// </summary>
        public static List<KeyValuePair<string, long>> SaysCount(ulong serverId, Dictionary<string, string> commandNames) =>
            _Statistics.Commands.Where(x => x.ServerId == serverId & commandNames.Any(c => c.Key == x.CommandName))
                .GroupBy(x => x.CommandName).Select(group => new {
                    CommandName = group.Key,
                    Count = group.LongCount()
                }).OrderByDescending(x => x.Count).ToDictionary(x => x.CommandName, x => x.Count).ToList();

    }

}
