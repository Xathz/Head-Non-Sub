using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.MySql.Core;
using HeadNonSub.Settings;

namespace HeadNonSub.Clients.Hangfire {

    public static class HangfireClient {

        public static void Load() {
            MySqlStorage storage = new MySqlStorage($"Server={SettingsManager.Configuration.HangfireHost};" +
                $"Database={SettingsManager.Configuration.HangfireDatabase};" +
                $"Uid={SettingsManager.Configuration.HangfireUsername};" +
                $"Pwd={SettingsManager.Configuration.HangfirePassword};" +
                $"AllowUserVariables=true;");

            GlobalConfiguration.Configuration.SetDataCompatibilityLevel(CompatibilityLevel.Version_170);
            GlobalConfiguration.Configuration.UseSimpleAssemblyNameTypeSerializer();
            GlobalConfiguration.Configuration.UseRecommendedSerializerSettings();
            // TODO Better way to log hangfire, it spams messages
            //GlobalConfiguration.Configuration.UseNLogLogProvider();
            GlobalConfiguration.Configuration.UseStorage(storage);

            _ = new BackgroundJobServer(new BackgroundJobServerOptions {
                SchedulePollingInterval = TimeSpan.FromSeconds(10),
                Queues = Queues.All
            });

            LoggingManager.Log.Info("Complete");
        }

        public static void Schedule<T>(Expression<Action<T>> methodCall, TimeSpan delay) => BackgroundJob.Schedule(methodCall, delay);

        public static void Schedule(Expression<Func<Task>> methodCall, TimeSpan delay) => BackgroundJob.Schedule(methodCall, delay);

        public static void Schedule(Expression<Action> methodCall, TimeSpan delay) => BackgroundJob.Schedule(methodCall, delay);

    }

}
