using System;
using System.Collections.Generic;
using System.Text;

namespace Gunucco.Entities
{
    /// <summary>
    /// Publish range of book, chapter or content
    /// </summary>
    public enum PublishRange : short
    {
        /// <summary>
        /// Anonymous can get information
        /// </summary>
        All = 101,

        /// <summary>
        /// Logined user only
        /// </summary>
        UserOnly = 201,

        /// <summary>
        /// Users has permission only
        /// </summary>
        Private = 501,
    }

    /// <summary>
    /// The location of media file
    /// </summary>
    public enum MediaSource : short
    {
        /// <summary>
        /// Media file is under self server
        /// </summary>
        Self,

        /// <summary>
        /// Media file is outside internet
        /// </summary>
        Outside,
    }
}
