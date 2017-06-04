using Gunucco.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace GunuccoSharp.CommandModels
{
    public class OauthCommand : CommandBase
    {
        internal OauthCommand(HttpClientBase client) : base(client)
        {
        }

        /// <summary>
        /// Create oauth code to open uri in browser
        /// </summary>
        /// <param name="scope">Oauth scope</param>
        /// <returns>Oauth code</returns>
        public async Task<OauthCode> CreateCodeAsync(Scope scope)
        {
            return await this.Client.Command<OauthCode>(new CommandInfo
            {
                Route = "user/login/oauthcode/create",
                Method = HttpMethod.Get,
                Data =
                {
                    { "scope", ((short)scope).ToString() },
                },
            });
        }

        /// <summary>
        /// Login with oauth code
        /// </summary>
        /// <param name="code">Oauth code object</param>
        /// <returns>Token</returns>
        public async Task<AuthorizationToken> LoginAsync(OauthCode code) => await this.LoginAsync(code.Code);

        /// <summary>
        /// Login with oauth code
        /// </summary>
        /// <param name="code">Oauth code</param>
        /// <returns>Token</returns>
        public async Task<AuthorizationToken> LoginAsync(string code)
        {
            return this.Client.AuthToken = await this.Client.Command<AuthorizationToken>(new CommandInfo
            {
                Route = "user/login/oauthcode",
                Method = HttpMethod.Post,
                Data =
                {
                    { "code", code },
                },
            });
        }
    }
}
