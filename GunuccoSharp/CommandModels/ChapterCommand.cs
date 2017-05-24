using Gunucco.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace GunuccoSharp.CommandModels
{
    public class ChapterCommand : CommandBase
    {
        internal ChapterCommand(HttpClientBase client) : base(client)
        {
        }

        public async Task<Chapter> CreateAsync(string name, int bookId)
        {
            return await this.Client.Command<Chapter>(new CommandInfo
            {
                Route = "chapter/create",
                Method = HttpMethod.Post,
                Data =
                {
                    { "name", name },
                    { "book_id", bookId.ToString() },
                },
            });
        }

        public async Task<Chapter> GetAsync(int id)
        {
            return await this.Client.Command<Chapter>(new CommandInfo
            {
                Route = "chapter/get/" + id,
                Method = HttpMethod.Get,
            });
        }

        public async Task<IEnumerable<Chapter>> GetChildrenAsync(int id)
        {
            return await this.Client.Command<IEnumerable<Chapter>>(new CommandInfo
            {
                Route = "chapter/get/" + id + "/children",
                Method = HttpMethod.Get,
            });
        }

        public async Task<IEnumerable<ContentMediaPair>> GetContentsAsync(int id)
        {
            return await this.Client.Command<IEnumerable<ContentMediaPair>>(new CommandInfo
            {
                Route = "chapter/get/" + id + "/contents",
                Method = HttpMethod.Get,
            });
        }

        public async Task<ApiMessage> UpdateAsync(Chapter chapter)
        {
            return await this.Client.Command<ApiMessage>(new CommandInfo
            {
                Route = "chapter/update",
                Method = HttpMethod.Put,
                Data =
                {
                    { "chapter", JsonConvert.SerializeObject(chapter) },
                },
            });
        }

        public async Task<ApiMessage> DeleteAsync(int id)
        {
            return await this.Client.Command<ApiMessage>(new CommandInfo
            {
                Route = "chapter/delete",
                Method = HttpMethod.Delete,
                Data =
                {
                    { "id", id.ToString() },
                },
            });
        }
    }
}
