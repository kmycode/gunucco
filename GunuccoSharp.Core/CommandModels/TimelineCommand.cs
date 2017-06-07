using Gunucco.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace GunuccoSharp.CommandModels
{
    public class TimelineCommand : CommandBase
    {
        internal TimelineCommand(HttpClientBase client) : base(client)
        {
        }

        public async Task<IEnumerable<TimelineItemContainer>> GetLocalAsync() => await this.GetLocalAsync(20, 0, int.MaxValue);
        public async Task<IEnumerable<TimelineItemContainer>> GetLocalAsync(int num) => await this.GetLocalAsync(num, 0, int.MaxValue);
        public async Task<IEnumerable<TimelineItemContainer>> GetLocalAsync(int num, int minId, int maxId)
        {
            return await this.Client.Command<IEnumerable<TimelineItemContainer>>(new CommandInfo
            {
                Route = "timeline/local",
                Method = HttpMethod.Get,
                Data =
                {
                    { "num", num.ToString() },
                    { "min_id", minId.ToString() },
                    { "max_id", maxId.ToString() },
                },
            });
        }
    }
}
