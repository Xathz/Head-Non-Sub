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
            string oldUsername, string newUsername, string oldUsernameDiscriminator, string newUsernameDiscriminator,
            string oldUserDisplay, string newUserDisplay, string oldUserAvatar, string newUserAvatar) {

            try {
                using (StatisticsContext statistics = new StatisticsContext()) {
                    UserChange item = new UserChange {
                        DateTime = dateTime,
                        ServerId = serverId,
                        UserId = userId,
                        ChangeType = changeType,
                        OldUsername = oldUsername,
                        OldUsernameDiscriminator = oldUsernameDiscriminator,
                        OldUserDisplay = oldUserDisplay,
                        OldUserAvatar = oldUserAvatar,
                        NewUsername = newUsername,
                        NewUsernameDiscriminator = newUsernameDiscriminator,
                        NewUserDisplay = newUserDisplay,
                        NewUserAvatar = newUserAvatar
                    };

                    statistics.UserChanges.Add(item);
                    statistics.SaveChanges();
                }
            } catch (Exception ex) {
                LoggingManager.Log.Error(ex);
            }
        }

        public static string GetUserChanges(ulong userId) {
            try {
                using (StatisticsContext statistics = new StatisticsContext()) {
                    IQueryable<UserChange> userChanges = statistics.UserChanges.AsNoTracking().Where(x => x.UserId == userId).OrderByDescending(x => x.DateTime);
                    StringBuilder builder = new StringBuilder();

                    foreach (UserChange userChange in userChanges) {
                        if (userChange.ChangeType != NameChangeType.None) {
                            builder.Append($"{userChange.DateTime.ToString(Constants.DateTimeFormatShort).ToLower()} utc");

                            if ((userChange.ChangeType & (userChange.ChangeType - 1)) != 0) {
                                builder.Append(Environment.NewLine);
                            }

                            List<string> changes = new List<string>();

                            if (userChange.ChangeType.HasFlag(NameChangeType.Username)) {
                                changes.Add($" ● [   user] {userChange.OldUsername} => {userChange.NewUsername}");
                            }

                            if (userChange.ChangeType.HasFlag(NameChangeType.Discriminator)) {
                                changes.Add($" ● [discrim] #{userChange.OldUsernameDiscriminator} => #{userChange.NewUsernameDiscriminator}");
                            }

                            if (userChange.ChangeType.HasFlag(NameChangeType.Display)) {
                                string oldUserDisplay = string.IsNullOrEmpty(userChange.OldUserDisplay) ? "<no nick>" : userChange.OldUserDisplay;
                                string newUserDisplay = string.IsNullOrEmpty(userChange.NewUserDisplay) ? "<no nick>" : userChange.NewUserDisplay;

                                changes.Add($" ● [   nick] {oldUserDisplay} => {newUserDisplay}");
                            }

                            if (userChange.ChangeType.HasFlag(NameChangeType.Avatar)) {
                                changes.Add($" ● [ avatar] Changed.");
                            }

                            builder.Append(string.Join(Environment.NewLine, changes));
                            builder.Append(Environment.NewLine);
                        }
                    }

                    return builder.ToString().Trim();
                }
            } catch (Exception ex) {
                LoggingManager.Log.Error(ex);
                return string.Empty;
            }
        }

    }

}
