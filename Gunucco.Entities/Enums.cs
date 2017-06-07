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
        WebClient = 32 | Read,

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

    /// <summary>
    /// The range of posting on list
    /// </summary>
    public enum PostTo : short
    {
        /// <summary>
        /// Post on none.
        /// </summary>
        None = 101,

        /// <summary>
        /// Post on global timeline. contains local timeline
        /// </summary>
        GlobalTimeline = 102,

        /// <summary>
        /// Post on local timeline. contains notification
        /// </summary>
        LocalTimeline = 103,

        /// <summary>
        /// Post on bookmarked user's notification column only.
        /// </summary>
        NotificationOnly = 104,
    }

    /// <summary>
    /// Operation target type. This value often effect to target id property
    /// </summary>
    public enum TargetType : short
    {
        /// <summary>
        /// Book
        /// </summary>
        Book = 101,

        /// <summary>
        /// Chapter
        /// </summary>
        Chapter = 102,

        /// <summary>
        /// Content
        /// </summary>
        Content = 103,
    }

    /// <summary>
    /// Target action
    /// </summary>
    public enum TargetAction : short
    {
        /// <summary>
        /// Ditch any actions
        /// </summary>
        Ditch = 101,

        /// <summary>
        /// Create item
        /// </summary>
        Create = 103,

        /// <summary>
        /// Update item information
        /// </summary>
        Update = 105,

        /// <summary>
        /// Delete item
        /// </summary>
        Delete = 107,
    }

    /// <summary>
    /// The range of item listed
    /// </summary>
    public enum TimelineListRange : int
    {
        /// <summary>
        /// No list appeared
        /// </summary>
        None = 0,

        /// <summary>
        /// Global timeline
        /// </summary>
        Global = 2,

        /// <summary>
        /// Local timeline
        /// </summary>
        Local = 4,

        /// <summary>
        /// all timelines
        /// </summary>
        All = int.MaxValue,     // maxvalue = all bits are '1'
    }
}
