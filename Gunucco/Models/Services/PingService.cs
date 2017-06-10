using Gunucco.Entities;
using Gunucco.Models.Database;
using Gunucco.Models.Entity;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Gunucco.Models.Services
{
    public static class PingService
    {
        private static Logger log = LogManager.GetCurrentClassLogger();

        private static int lastContainerId = 0;

        public static void StartService()
        {
            log.Info("Start ping service");

            // get last timeline id
            using (var db = new MainContext())
            {
                lastContainerId = db.TimelineItem.LastOrDefault()?.Id ?? 0;
            }

            Task.Run(async () =>
            {
                while (true)
                {
                    try
                    {
                        Task.Delay(1000 * 60 * 20).Wait();      // 20 minutes

                        var last = new TimelineModel().GetLocalItems(1, 0, int.MaxValue).FirstOrDefault();
                        if (last != null)
                        {
                            if (last.TimelineItem.Id != lastContainerId)
                            {
                                lastContainerId = last.TimelineItem.Id;
                                await SendPing();
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        log.Error(e, "Ping service error:");
                        Task.Delay(1000 * 60 * 1).Wait();
                    }
                }
            });
        }

        private static async Task SendPing()
        {
            if (!Config.IsDebugMode)
            {
                try
                {
                    var client = new HttpClient();
                    var content = new StringContent($@"<?xml version=""1.0"" encoding=""UTF-8""?>
<methodCall>
<methodName>weblogUpdates.ping</methodName>
<params>
<param><value>Gunucco (path={Config.ServerPath})</value></param>
<param><value>{Config.ServerPath}/</value></param>
</params>
</methodCall>", Encoding.UTF8, "application/xml");
                    var response = await client.PostAsync("http://blogsearch.google.co.jp/ping/RPC2", content);
                    var result = await response.Content.ReadAsStringAsync();
                    log.Info("Ping completed. Server returns: " + result);
                }
                catch (Exception e)
                {
                    log.Error(e, "Send ping failed:");
                }
            }
        }
    }
}
