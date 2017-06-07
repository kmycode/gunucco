using Gunucco.Entities;
using GunuccoSharp.CommandModels;
using System;
using System.Net.Http;

namespace GunuccoSharp
{
    public class GunuccoSharpClient : HttpClientBase
    {
        /// <summary>
        /// Get server commands
        /// </summary>
        public ServerCommand Server => new ServerCommand(this);

        /// <summary>
        /// Get authorization management commands
        /// </summary>
        public AuthorizationCommand Auth => new AuthorizationCommand(this);

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

        /// <summary>
        /// Get content commands
        /// </summary>
        public ContentCommand Content => new ContentCommand(this);

        /// <summary>
        /// Get timeline commands
        /// </summary>
        public TimelineCommand Timeline => new TimelineCommand(this);
    }
}
