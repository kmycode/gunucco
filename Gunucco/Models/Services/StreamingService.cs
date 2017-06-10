using Gunucco.Entities;
using Gunucco.Models.Entity;
using Microsoft.AspNetCore.Http;
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

        public static Streaming GlobalTimeline { get; } = new Streaming();

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
                        await Task.Delay(10000);
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
                    var removeResponses = new Collection<HttpResponse>();

                    // write response
                    foreach (var r in this.Responses)
                    {
                        try
                        {
                            await r.WriteAsync(json, Encoding.UTF8);
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
                catch (Exception ex)
                {
                    log.Error(ex, "Timeline streaming error occured.");
                }
            }
        }
    }
}
