using Gunucco.Entities;
using Gunucco.Models.Database;
using Gunucco.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gunucco.Models.Utils
{
    public static class TimelineUtil
    {
        public static void AddTimeline(MainContext db, int userId, DateTime dt, TargetType type, TargetAction action, int targetId, TimelineListRange range, int? actionTargetId = null)
        {
            db.TimelineItem.Add(new TimelineItem
            {
                ServerPath = Config.ServerPath,
                ListRange = range,
                TargetAction = action,
                TargetId = targetId,
                ActionTargetId = actionTargetId,
                TargetType = type,
                Updated = dt,
                UserId = userId,
            });
        }

        public static void AddBookTimeline(MainContext db, AuthorizationData authData, Book book, TargetAction action, TimelineListRange range = TimelineListRange.All, int? actionTargetId = null)
        {
            AddTimeline(db,
                        authData.User.Id,
                        action == TargetAction.Create ? book.Created : book.LastModified,
                        TargetType.Book,
                        action,
                        book.Id,
                        range,
                        actionTargetId);
        }

        public static void AddChapterTimeline(MainContext db, AuthorizationData authData, Chapter chapter, TargetAction action, TimelineListRange range = TimelineListRange.All, int? actionTargetId = null)
        {
            AddTimeline(db,
                        authData.User.Id,
                        DateTime.Now,
                        TargetType.Chapter,
                        action,
                        chapter.Id,
                        range,
                        actionTargetId);
        }

        public static void AddContentTimeline(MainContext db, AuthorizationData authData, Content content, TargetAction action, TimelineListRange range = TimelineListRange.All, int? actionTargetId = null)
        {
            AddTimeline(db,
                        authData.User.Id,
                        action == TargetAction.Create ? content.Created : content.LastModified,
                        TargetType.Content,
                        action,
                        content.Id,
                        range,
                        actionTargetId);
        }
    }
}
