using Gunucco.Models.Database;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gunucco.Models.Services
{
    public static class DBCleanerService
    {
        private static Logger log = LogManager.GetCurrentClassLogger();

        public static void StartServices()
        {
            Task.Run(() => _userSessionCleaner.StartService());
            Task.Run(() => _oauthCodeCleaner.StartService());
        }

        private static DBCleaner _userSessionCleaner = new DBCleaner
        {
            NextCleaningTiming = dt => dt.AddHours(1),
            CleaningAction = async db =>
            {
                var now = DateTime.Now;

                // 6 hours after expired to output 'token already expired' error when user logined old token
                var removeDateTime = now.AddHours(-6);

                log.Info("[Start] Cleaning expired sessions service");
                log.Info("    delete sessions until " + removeDateTime.ToString("yyyy/MM/dd HH:mm:ss") + ".");

                int deleteCount = 0;
                try
                {
                    var expireds = db.UserSession.Where(e => e.ExpireDateTime < removeDateTime);
                    db.UserSession.RemoveRange(expireds);
                    deleteCount = expireds.Count();

                    await db.SaveChangesAsync();
                }
                catch (Exception e)
                {
                    log.Error(e, "    exception occured while using database.");
                }

                log.Info("    deleted " + (deleteCount == 0 ? "no" : deleteCount.ToString()) + " session" + (deleteCount <= 1 ? "." : "s."));
                log.Info("[End] Cleaning expired sessions");
            },
        };

        private static DBCleaner _oauthCodeCleaner = new DBCleaner
        {
            NextCleaningTiming = dt => dt.AddHours(2),
            CleaningAction = async db =>
            {
                var now = DateTime.Now;

                // 6 hours after expired to output 'token already expired' error when user logined old token
                var removeDateTime = now;

                log.Info("[Start] Cleaning expired oauth-codes service");
                log.Info("    delete oauth-codes until " + removeDateTime.ToString("yyyy/MM/dd HH:mm:ss") + ".");

                int deleteCount = 0;
                try
                {
                    var expireds = db.OauthCode.Where(e => e.ExpireDateTime < removeDateTime);
                    db.OauthCode.RemoveRange(expireds);
                    deleteCount = expireds.Count();

                    await db.SaveChangesAsync();
                }
                catch (Exception e)
                {
                    log.Error(e, "    exception occured while using database.");
                }

                log.Info("    deleted " + (deleteCount == 0 ? "no" : deleteCount.ToString()) + " code" + (deleteCount <= 1 ? "." : "s."));
                log.Info("[End] Cleaning expired oauth-codes");
            },
        };

        private class DBCleaner
        {
            public DateTime NextCleanTime { get; set; } = DateTime.MinValue;

            public Func<DateTime, DateTime> NextCleaningTiming { get; set; }

            public Func<MainContext, Task> CleaningAction { get; set; }

            private readonly object locker = new object();

            public async void StartService()
            {
                while (true)
                {
                    try
                    {
                        await this.CleanAsync();
                        Task.Delay(1000 * 60 * 2).Wait();
                    }
                    catch (Exception e)
                    {
                        log.Error(e, "Service failed");
                    }
                }
            }

            public async Task CleanAsync()
            {
                var now = DateTime.Now;
                lock (this.locker)
                {
                    if (this.NextCleanTime > now) return;
                }

                this.NextCleanTime = this.NextCleaningTiming(now);

                using (var db = new MainContext())
                {
                    await this.CleaningAction(db);
                }
            }
        }
    }
}
