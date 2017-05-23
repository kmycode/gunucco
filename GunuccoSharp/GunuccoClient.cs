using Gunucco.Entities;
using GunuccoSharp.CommandModels;
using System;
using System.Net.Http;

namespace GunuccoSharp
{
    public class GunuccoSharpClient : HttpClientBase
    {
        /// <summary>
        /// Get user commands
        /// </summary>
        public UserCommand User => new UserCommand(this);

        /// <summary>
        /// Get book commands
        /// </summary>
        public BookCommand Book => new BookCommand(this);

        /// <summary>
        /// Get chapter commands
        /// </summary>
        public ChapterCommand Chapter => new ChapterCommand(this);
    }
}
