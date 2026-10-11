using System;
using System.Collections.Generic;
using System.Linq;
using HeadNonSub.Statistics.Tables;
using Microsoft.EntityFrameworkCore;

namespace HeadNonSub.Statistics {

    public static partial class StatisticsManager {

        private static readonly Dictionary<string, string> _CommandNameMap = new Dictionary<string, string>() {
            { "TTSays", "tt2468 Says" }, { "TenTwentyFourSays", "1024x768 Says" }, { "AmandaSays", "Amanda Says" },
            { "SataSays", "Satazero Says" }, { "JiberSays", "jiberjiber Says" }
        };

        /// <summary>
        /// Insert a command that was ran into the database.
        /// </summary>
        public static void InsertCommand(DateTime dateTime, ulong serverId, ulong channelId, ulong userId, string username, string userDisplay,
                                            ulong messageId, string message, string command, string parameters, ulong? replyMessageId) {
            try {
                using StatisticsContext statistics = new StatisticsContext();

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
                    Parameters = parameters,
                    ReplyMessageId = replyMessageId
                };

                statistics.Commands.Add(item);
                statistics.SaveChanges();
            } catch (Exception ex) {
                LoggingManager.Log.Error(ex);
            }
        }

        /// <summary>
        /// Get the number of times the true command was executed.
        /// </summary>
        public static long GetTrueCount(ulong serverId) {
            try {
                using StatisticsContext statistics = new StatisticsContext();
                return statistics.Commands.AsNoTracking().Where(x => x.ServerId == serverId & x.CommandName == "ThatsTrue").LongCount();
            } catch (Exception ex) {
                LoggingManager.Log.Error(ex);
                return 0;
            }
        }

        /// <summary>
        /// Get the number of times each says command was executed order by count.
        /// </summary>
        public static List<KeyValuePair<string, long>> GetSaysCount(ulong serverId) {
            try {
                using StatisticsContext statistics = new StatisticsContext();

                return statistics.Commands.AsNoTracking().Where(x => x.ServerId == serverId & _CommandNameMap.Any(c => c.Key == x.CommandName))
                        .GroupBy(x => x.CommandName).Select(g => new {
                            CommandName = _CommandNameMap[g.Key],
                            Count = g.LongCount()
                        }).OrderByDescending(x => x.Count).ToDictionary(x => x.CommandName, x => x.Count).ToList();
            } catch (Exception ex) {
                LoggingManager.Log.Error(ex);
                return new List<KeyValuePair<string, long>>();
            }
        }

    }

}
