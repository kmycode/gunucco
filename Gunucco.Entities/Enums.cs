using System;
using System.Collections.Generic;
using System.Text;

namespace Gunucco.Entities
{
    /// <summary>
    /// Authentication scope
    /// </summary>
    [Flags]
    public enum Scope : int
    {
        /// <summary>
        /// No scope is set.
        /// </summary>
        None = 0,

        /// <summary>
        /// User id, name or so on only to identity user.
        /// </summary>
        ReadUserIdentity = 1,

        /// <summary>
        /// Get user's private items.
        /// </summary>
        Read = 2,

        /// <summary>
        /// Write user's items
        /// </summary>
        Write = 4,

        /// <summary>
        /// Write user's identity data ex: user name or password.
        /// </summary>
        WriteUserIdentity = 8,

        /// <summary>
        /// Write user's dangerous identity ex: delete user.
        /// </summary>
        WriteUserDangerousIdentity = 16 | WriteUserIdentity,

        /// <summary>
        /// Logined by official web client. It arrows apis own web client.
        /// </summary>
        WebClient = 32 | ReadUserIdentity | Read | Write | WriteUserIdentity | WriteUserDangerousIdentity,

        /// <summary>
        /// Logined with text id and password api. It arrows identity, read or write, however is deprecated.
        /// </summary>
#if !GUNUCCO
        [Obsolete("This scope is for logining with text id and password. Use oauth to login.")]
#endif
#if !DEBUG && !UNITTEST
        LegacyFull = ReadUserIdentity | Read | Write,
#else
        LegacyFull = ReadUserIdentity | Read | Write | WriteUserDangerousIdentity,
#endif
    }

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
        Self = 101,

        /// <summary>
        /// Media file is outside internet
        /// </summary>
        Outside = 102,
    }
}
