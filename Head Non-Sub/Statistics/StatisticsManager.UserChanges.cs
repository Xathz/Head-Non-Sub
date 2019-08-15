using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HeadNonSub.Statistics.Tables;
using Microsoft.EntityFrameworkCore;

namespace HeadNonSub.Statistics {

    public static partial class StatisticsManager {

        /// <summary>
        /// Insert a user change event into the database.
        /// </summary>
        public static void InsertUserChange(DateTime dateTime, ulong? serverId, ulong userId, NameChangeType changeType,
            string oldUsername, string newUsername, string oldUsernameDiscriminator, string newUsernameDiscriminator, string oldUserDisplay, string newUserDisplay,
            string backblazeAvatarBucket, string backblazeAvatarFilename, string backblazeAvatarUrl) {

            try {
                using (StatisticsContext statistics = new StatisticsContext()) {
                    UserChange item = new UserChange {
                        DateTime = dateTime,
                        ServerId = serverId,
                        UserId = userId,
                        ChangeType = changeType,
                        OldUsername = oldUsername,
                        NewUsername = newUsername,
                        OldUsernameDiscriminator = oldUsernameDiscriminator,
                        NewUsernameDiscriminator = newUsernameDiscriminator,
                        OldUserDisplay = oldUserDisplay,
                        NewUserDisplay = newUserDisplay,
                        BackblazeAvatarBucket = backblazeAvatarBucket,
                        BackblazeAvatarFilename = backblazeAvatarFilename,
                        BackblazeAvatarUrl = backblazeAvatarUrl
                    };

                    statistics.UserChanges.Add(item);
                    statistics.SaveChanges();
                }
            } catch (Exception ex) {
                LoggingManager.Log.Error(ex);
            }
        }

        /// <summary>
        /// Get all changes from a user.
        /// </summary>
        /// <param name="serverId">Server id.</param>
        /// <param name="userId">User id.</param>
        public static string GetUserChanges(ulong serverId, ulong userId) {
            try {
                using (StatisticsContext statistics = new StatisticsContext()) {
                    IQueryable<UserChange> userChanges = statistics.UserChanges.AsNoTracking().OrderByDescending(x => x.DateTime).Where(x => x.UserId == userId);
                    StringBuilder builder = new StringBuilder();

                    foreach (UserChange userChange in userChanges) {
                        if (userChange.ChangeType != NameChangeType.None) {
                            List<string> changes = new List<string>();

                            if (userChange.ChangeType.HasFlag(NameChangeType.Username)) {
                                changes.Add($" ● [   user] {userChange.OldUsername} => {userChange.NewUsername}");
                            }

                            if (userChange.ChangeType.HasFlag(NameChangeType.Discriminator)) {
                                changes.Add($" ● [discrim] #{userChange.OldUsernameDiscriminator} => #{userChange.NewUsernameDiscriminator}");
                            }

                            if (userChange.ChangeType.HasFlag(NameChangeType.Display)) {
                                if (userChange.ServerId.HasValue && userChange.ServerId.Value == serverId) {
                                    string oldUserDisplay = string.IsNullOrEmpty(userChange.OldUserDisplay) ? "<no nick>" : userChange.OldUserDisplay;
                                    string newUserDisplay = string.IsNullOrEmpty(userChange.NewUserDisplay) ? "<no nick>" : userChange.NewUserDisplay;

                                    changes.Add($" ● [   nick] {oldUserDisplay} => {newUserDisplay}");
                                }
                            }

                            if (userChange.ChangeType.HasFlag(NameChangeType.Avatar)) {
                                if (!string.IsNullOrWhiteSpace(userChange.BackblazeAvatarUrl)) {
                                    changes.Add($" ● [ avatar] {userChange.BackblazeAvatarUrl}");
                                }
                            }

                            if (changes.Count > 0) {
                                builder.Append($"{userChange.DateTime.ToString(Constants.DateTimeFormatShort).ToLower()} utc");

                                if ((userChange.ChangeType & (userChange.ChangeType - 1)) != 0) {
                                    builder.Append(Environment.NewLine);
                                }

                                builder.Append(string.Join(Environment.NewLine, changes));
                                builder.Append(Environment.NewLine);
                            }
                        }
                    }

                    return builder.ToString().Trim();
                }
            } catch (Exception ex) {
                LoggingManager.Log.Error(ex);
                return string.Empty;
            }
        }

        /// <summary>
        /// Get the top users who have changes by count.
        /// </summary>
        /// <param name="count">How many users are in the 'top'.</param>
        /// <returns>List of (user id, count)</returns>
        public static List<KeyValuePair<ulong, long>> GetTopChangers(int count) {
            try {
                using (StatisticsContext statistics = new StatisticsContext()) {
                    return statistics.UserChanges.AsNoTracking().OrderByDescending(x => x.DateTime)
                        .Where(x => x.ChangeType != NameChangeType.None)
                        .GroupBy(x => x.UserId)
                        .Select(g => new {
                            UserId = g.Key,
                            Count = g.LongCount()
                        }).OrderByDescending(x => x.Count)
                        .ToDictionary(x => x.UserId, x => x.Count)
                        .Take(count)
                        .ToList();
                }
            } catch (Exception ex) {
                LoggingManager.Log.Error(ex);
                return new List<KeyValuePair<ulong, long>>();
            }
        }

    }

}
