using Gunucco.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace GunuccoSharp.CommandModels
{
    public class ContentCommand : CommandBase
    {
        internal ContentCommand(HttpClientBase client) : base(client)
        {
        }

        public async Task<ContentMediaPair> CreateTextAsync(int chapterId, string text)
        {
            return await this.Client.Command<ContentMediaPair>(new CommandInfo
            {
                Route = "content/create/text",
                Method = HttpMethod.Post,
                Data =
                {
                    { "chapter_id", chapterId.ToString() },
                    { "text", text },
                },
            });
        }

        public async Task<ContentMediaPair> CreateImageAsync(int chapterId, MediaExtension extension, Stream data)
        {
            var extensionStr = extension == MediaExtension.Gif ? "gif" : extension == MediaExtension.Jpeg ? "jpeg" : extension == MediaExtension.Png ? "png" : throw new GunuccoException("Invalid extension value.");
            return await this.Client.Command<ContentMediaPair>(new CommandInfo
            {
                Route = "content/create/image",
                Method = HttpMethod.PostMedia,
                Data =
                {
                    { "source", ((short)MediaSource.Self).ToString() },
                    { "extension", extensionStr },
                    { "chapter_id", chapterId.ToString() },
                },
                Media =
                {
                    new Tuple<string, Stream, string>("data", data, "image." + extensionStr),
                },
            });
        }

        public async Task<ContentMediaPair> CreateImageAsync(int chapterId, string imageUri)
        {
            return await this.Client.Command<ContentMediaPair>(new CommandInfo
            {
                Route = "content/create/image",
                Method = HttpMethod.Post,
                Data =
                {
                    { "source", ((short)MediaSource.Outside).ToString() },
                    { "chapter_id", chapterId.ToString() },
                    { "data_uri", imageUri },
                },
            });
        }

        public async Task<ContentMediaPair> GetAsync(int id)
        {
            return await this.Client.Command<ContentMediaPair>(new CommandInfo
            {
                Route = "content/" + id,
                Method = HttpMethod.Get,
            });
        }

        public async Task<ApiMessage> UpdateAsync(Content content)
        {
            return await this.Client.Command<ApiMessage>(new CommandInfo
            {
                Route = "content/update",
                Method = HttpMethod.Put,
                Data =
                {
                    { "content", JsonConvert.SerializeObject(content) },
                },
            });
        }

        public async Task<ApiMessage> DeleteAsync(int id)
        {
            return await this.Client.Command<ApiMessage>(new CommandInfo
            {
                Route = "content/delete",
                Method = HttpMethod.Delete,
                Data =
                {
                    { "id", id.ToString() },
                },
            });
        }
    }
}
