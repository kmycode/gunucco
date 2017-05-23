using Gunucco.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace GunuccoSharp.CommandModels
{
    public class BookCommand : CommandBase
    {
        internal BookCommand(HttpClientBase client) : base(client)
        {
        }

        public async Task<Book> CreateAsync(string name)
        {
            return await this.Client.Command<Book>(new CommandInfo
            {
                Route = "book/create",
                Method = HttpMethod.Post,
                Data =
                {
                    { "name", name },
                },
            });
        }

        public async Task<Book> GetAsync(int bookId)
        {
            return await this.Client.Command<Book>(new CommandInfo
            {
                Route = "book/get/" + bookId,
                Method = HttpMethod.Get,
            });
        }

        public async Task<IEnumerable<Chapter>> GetChaptersAsync(int bookId)
        {
            return await this.Client.Command<IEnumerable<Chapter>>(new CommandInfo
            {
                Route = "book/get/" + bookId + "/chapters",
                Method = HttpMethod.Get,
            });
        }

        public async Task<IEnumerable<Chapter>> GetRootChaptersAsync(int bookId)
        {
            return await this.Client.Command<IEnumerable<Chapter>>(new CommandInfo
            {
                Route = "book/get/" + bookId + "/chapters/root",
                Method = HttpMethod.Get,
            });
        }

        public async Task<IEnumerable<Book>> GetUserBooksAsync(int userId)
        {
            return await this.Client.Command<IEnumerable<Book>>(new CommandInfo
            {
                Route = "book/get/user/" + userId,
                Method = HttpMethod.Get,
            });
        }

        public async Task<ApiMessage> DeleteAsync(int id)
        {
            return await this.Client.Command<ApiMessage>(new CommandInfo
            {
                Route = "book/delete",
                Method = HttpMethod.Delete,
                Data =
                {
                    { "id", id.ToString() },
                },
            });
        }

        public Task DeleteAsync(object id)
        {
            throw new NotImplementedException();
        }
    }
}
