using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Gunucco.Entities;

namespace GunuccoSharp.CommandModels
{
    public class AuthorizationCommand : CommandBase
    {
        internal AuthorizationCommand(HttpClientBase client) : base(client)
        {
        }

        public async Task<IEnumerable<UserSessionData>> GetListAsync()
        {
            return await this.Client.Command<IEnumerable<UserSessionData>>(new CommandInfo
            {
                Route = "auth/list",
                Method = HttpMethod.Get,
            });
        }

        public async Task<ApiMessage> DeleteAsync(string idHash)
        {
            return await this.Client.Command<ApiMessage>(new CommandInfo
            {
                Route = "auth/delete",
                Method = HttpMethod.Delete,
                Data =
                {
                    { "id_hash", idHash },
                },
            });
        }
    }
}
