using Gunucco.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace GunuccoSharp.CommandModels
{
    public class LoginCommand : CommandBase
    {
        internal LoginCommand(HttpClientBase client) : base(client)
        {
        }

        /// <summary>
        /// Oauth command set
        /// </summary>
        public OauthCommand Oauth => new OauthCommand(this.Client);

        /// <summary>
        /// Login user
        /// </summary>
        /// <param name="id">user text id</param>
        /// <param name="password">user password</param>
        /// <returns>Token for use apis needed authentication</returns>
        public async Task<AuthorizationToken> WithIdAndPasswordAsync(string id, string password)
        {
            return this.Client.AuthToken = await this.Client.Command<AuthorizationToken>(new CommandInfo
            {
                Route = "user/login/idandpassword",
                Method = HttpMethod.Post,
                Data =
                {
                    { "id", id },
                    { "password", password },
                },
            });
        }
    }
}
