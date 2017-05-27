using Gunucco.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace GunuccoSharp.CommandModels
{
    public class UserCommand : CommandBase
    {
        public UserCommand(HttpClientBase client) : base(client)
        {
        }

        /// <summary>
        /// Get login commands object
        /// </summary>
        public LoginCommand Login => new LoginCommand(this.Client);

        /// <summary>
        /// Create new user
        /// </summary>
        /// <param name="id">user text id</param>
        /// <param name="password">user password. upper 6 chars</param>
        /// <returns>User object</returns>
        public async Task<User> CreateAsync(string id, string password)
        {
            return await this.Client.Command<User>(new CommandInfo
            {
                Route = "user/create",
                Method = HttpMethod.Post,
                Data =
                {
                    { "id", id },
                    { "password", password },
                },
            });
        }

        /// <summary>
        /// Delete current user
        /// </summary>
        /// <returns>Api result</returns>
        public async Task<ApiMessage> DeleteAsync()
        {
            return await this.Client.Command<ApiMessage>(new CommandInfo
            {
                Route = "user/delete",
                Method = HttpMethod.Delete,
            });
        }

        /// <summary>
        /// Get user all books
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<IEnumerable<Book>> GetBooksAsync(int userId)
        {
            return await this.Client.Command<IEnumerable<Book>>(new CommandInfo
            {
                Route = "user/" + userId + "/books",
                Method = HttpMethod.Get,
            });
        }
    }
}
