using Gunucco.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace GunuccoSharp.CommandModels
{
    public class ServerCommand : CommandBase
    {
        internal ServerCommand(HttpClientBase client) : base(client)
        {
        }

        public async Task<string> GetVersionAsync()
        {
            var mes = await this.Client.Command<ApiMessage>(new CommandInfo
            {
                Route = "server/version",
                Method = HttpMethod.Get,
            });
            return mes.Message;
        }
    }
}
