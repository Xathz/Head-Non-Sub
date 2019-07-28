using System;
using System.Linq;
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

                    statistics.NameChanges.Add(item);
                    statistics.SaveChanges();
                }
            } catch (Exception ex) {
                LoggingManager.Log.Error(ex);
            }
        }

    }

}
