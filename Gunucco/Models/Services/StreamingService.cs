using Gunucco.Entities;
using Gunucco.Models.Database;
using Gunucco.Models.Entity;
using GunuccoSharp;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gunucco.Models.Services
{
    public static class StreamingService
    {
        private static Logger log = LogManager.GetCurrentClassLogger();

        public static Streaming LocalTimeline { get; } = new Streaming();

        public static GlobalStreaming GlobalTimeline { get; } = new GlobalStreaming();

        public class Streaming
        {
            public Collection<HttpResponse> Responses = new Collection<HttpResponse>();

            public Streaming()
            {
                Task.Run(async () =>
                {
                    while (true)
                    {
                        await this.SignalAsync();
                        await Task.Delay(!Config.IsDebugMode ? 60000 : 10000);
                    }
                });
            }

            public async Task SignalAsync()
            {
                var removeResponses = new Collection<HttpResponse>();

                // write response
                foreach (var r in this.Responses)
                {
                    try
                    {
                        await r.WriteAsync("event: signal\n\n", Encoding.UTF8);
                        await r.Body.FlushAsync();
                    }
                    catch
                    {
                        removeResponses.Add(r);
                    }
                }

                // delete errored response
                foreach (var r in removeResponses)
                {
                    this.Responses.Remove(r);
                }
            }

            public async Task WriteAsync(TimelineItem item)
            {
                try
                {
                    var json = "data: " + JsonConvert.SerializeObject(TimelineModel.GetContainer(item));
                    json = json.Replace("\\", "\\\\").Replace("\n", "\\n") + "\n\n";

                    this.WriteAsync(json);
                }
                catch (Exception ex)
                {
                    log.Error(ex, "Timeline streaming error occured.");
                }
            }

            protected async Task WriteAsync(string text)
            {
                var removeResponses = new Collection<HttpResponse>();

                // write response
                foreach (var r in this.Responses)
                {
                    try
                    {
                        await r.WriteAsync(text, Encoding.UTF8);
                        await r.Body.FlushAsync();
                    }
                    catch
                    {
                        removeResponses.Add(r);
                    }
                }

                // delete errored response
                foreach (var r in removeResponses)
                {
                    this.Responses.Remove(r);
                }
            }
        }

        public class GlobalStreaming : Streaming
        {
            private readonly Collection<GlobalServer> servers = new Collection<GlobalServer>();

            private readonly object listLocker = new object();

            public GlobalStreaming()
            {
                // initialize list
                this.UpdateList();

                // update list per 1 hour
                Task.Run(() =>
                {
                    while (true)
                    {
                        this.UpdateList();
                        Task.Delay(1000 * 60 * 60).Wait();      // 1 hour
                    }
                });
            }

            public void AddServer(string path)
            {
                using (var db = new MainContext())
                {
                    this.AddServer(path, db);
                }
            }

            public void AddServer(string path, MainContext db)
            {
                var server = new GlobalServer
                {
                    ServerPath = path,
                    IsBlocking = false,
                };
                db.GlobalServer.Add(server);
                db.SaveChanges();

                this.servers.Add(server);
                this.StartListening(server);
            }

            private void UpdateList()
            {
                lock (this.listLocker)
                {
                    using (var db = new MainContext())
                    {
                        var list = db.GlobalServer.Where(i => true);
                        list.Load();

                        this.servers.Clear();
                        foreach (var item in list)
                        {
                            this.servers.Add(item);
                        }
                    }
                }
            }

            public void RemoveServer(string path)
            {
                using (var db = new MainContext())
                {
                    var server = db.GlobalServer.SingleOrDefault(item => item.ServerPath == path);
                    if (server != null)
                    {
                        db.GlobalServer.Remove(server);
                        db.SaveChanges();
                    }

                    server = this.servers.SingleOrDefault(item => item.ServerPath == path);
                    if (server != null)
                    {
                        this.servers.Remove(server);
                    }
                }
            }

            public void StartListening()
            {
                lock (this.listLocker)
                {
                    log.Info("Start global timeline listening");
                    foreach (var server in this.servers)
                    {
                        this.StartListening(server);
                    }
                }
            }

            private void StartListening(GlobalServer server)
            {
                Action<string> task = async (path) =>
                {
                    log.Info("Start global timeline listening: '" + path + "'");
                    int failedCount = 0;

                    while (failedCount < 12)
                    {
                        try
                        {
                            var client = new GunuccoSharpClient
                            {
                                ServicePath = path,
                            };
                            var streaming = client.Timeline.GetLocalStreaming(new StreamingReceiver(async (item) =>
                            {
                                // streaming to local server
                                failedCount = 0;
                                if (item.TimelineItem.ServerPath != Config.ServerPath &&
                                    (item.TimelineItem.ListRange.HasFlag(TimelineListRange.Global) || Config.IsDebugMode))
                                {
                                    await this.WriteAsync("data: " + JsonConvert.SerializeObject(item) + "\n\n");
                                }
                            }));
                            await streaming.ReceiveLoopAsync();
                        }
                        catch (Exception e)
                        {
                            log.Error(e, "Listening global timeline failed: '" + path + "'");
                            failedCount++;
                            await Task.Delay(1000);
                        }
                    }

                    log.Info("Listening global timeline ended because error occured some times: '" + path + "'");
                };

                if (!server.IsBlocking)
                {
                    task(server.ServerPath);
                }
                else
                {
                    log.Info("Listening global timeline is aborted because of IsBlocking property: '" + server.ServerPath + "'");
                }
            }

            private class StreamingReceiver : IStreamingReceiver<TimelineItemContainer>
            {
                private Action<TimelineItemContainer> onNext;

                public StreamingReceiver(Action<TimelineItemContainer> onNext)
                {
                    this.onNext = onNext;
                }

                public void OnNext(TimelineItemContainer item)
                {
                    this.onNext(item);
                }
            }
        }
    }
}
