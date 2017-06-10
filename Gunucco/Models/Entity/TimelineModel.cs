using Gunucco.Common;
using Gunucco.Entities;
using Gunucco.Models.Database;
using Gunucco.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Gunucco.Models.Entity
{
    public class TimelineModel
    {
        public AuthorizationData AuthData { get; set; }

        public IEnumerable<TimelineItemContainer> GetItems(int num, int minId, int maxId)
        {
            using (var db = new MainContext())
            {
                var items = this.GetItems(db, i => true, num, minId, maxId);
                items.Load();
                return this.CheckTimelineItemContainerProperties(items.ToArray());
            }
        }

        public IQueryable<TimelineItemContainer> GetItems(MainContext db, Expression<Func<TimelineItem, bool>> predicate, int num, int minId, int maxId)
        {
            this.CheckParameters(num, minId, maxId);
            var a = db.TimelineItem.Where(i => i.Id <= maxId && i.Id >= minId)
                                  .Where(predicate)
                                  .OrderByDescending(i => i.Id)
                                  .Take(num)
                                  .GroupJoin(db.User, i => i.UserId, u => u.Id, (i, us) => new TimelineItemContainer { TimelineItem = i, User = us.FirstOrDefault(), })
                                  .GroupJoin(db.Content, i => i.TimelineItem.TargetType == TargetType.Content ? i.TimelineItem.TargetId : 0, c => c.Id, (i, cs) => new TimelineItemContainer { TimelineItem = i.TimelineItem, User = i.User, Book = i.Book, Chapter = i.Chapter, ContentMediaPair = new ContentMediaPair { Content = cs.FirstOrDefault(), } })
                                  .GroupJoin(db.Media, i => i.TimelineItem.TargetType == TargetType.Content ? i.TimelineItem.TargetId : 0, m => m.ContentId, (i, ms) => new TimelineItemContainer { TimelineItem = i.TimelineItem, User = i.User, Book = i.Book, Chapter = i.Chapter, ContentMediaPair = new ContentMediaPair { Content = i.ContentMediaPair.Content, Media = ms.FirstOrDefault(), } })
                                  .GroupJoin(db.Chapter, i => i.TimelineItem.TargetType == TargetType.Chapter ? i.TimelineItem.TargetId :
                                                              i.TimelineItem.TargetType == TargetType.Content && i.ContentMediaPair != null && i.ContentMediaPair.Content != null ? i.ContentMediaPair.Content.ChapterId : 0, c => c.Id, (i, cs) => new TimelineItemContainer { TimelineItem = i.TimelineItem, User = i.User, Book = i.Book, Chapter = cs.FirstOrDefault(), ContentMediaPair = i.ContentMediaPair, })
                                  .GroupJoin(db.Book, i => i.TimelineItem.TargetType == TargetType.Book ? i.TimelineItem.TargetId :
                                                           i.TimelineItem.TargetType == TargetType.Chapter && i.Chapter != null ? i.Chapter.BookId :
                                                           i.TimelineItem.TargetType == TargetType.Content && i.Chapter != null ? i.Chapter.BookId : 0, b => b.Id, (i, bs) => new TimelineItemContainer { TimelineItem = i.TimelineItem, User = i.User, Book = bs.FirstOrDefault(), Chapter = i.Chapter, ContentMediaPair = i.ContentMediaPair, });
            return a;
        }

        public static TimelineItemContainer GetContainer(TimelineItem item)
        {
            var container = new TimelineItemContainer
            {
                TimelineItem = item,
            };

            using (var db = new MainContext())
            {
                switch (item.TargetType)
                {
                    case TargetType.Book:
                        container.Book = db.Book.Find(item.TargetId);
                        break;
                    case TargetType.Chapter:
                        container.Chapter = db.Chapter.Find(item.TargetId);
                        if (container.Chapter != null)
                        {
                            container.Book = db.Book.Find(container.Chapter.BookId);
                        }
                        break;
                    case TargetType.Content:
                        var pss = db.Content.Where(c => c.Id == item.TargetId).GroupJoin(db.Media, c => c.Id, m => m.ContentId, (c, ms) => new ContentMediaPair { Content = c, Media = ms.FirstOrDefault(), });
                        container.ContentMediaPair = db.Content.Where(c => c.Id == item.TargetId).GroupJoin(db.Media, c => c.Id, m => m.ContentId, (c, ms) => new ContentMediaPair { Content = c, Media = ms.FirstOrDefault(), }).SingleOrDefault();
                        if (container.ContentMediaPair != null)
                        {
                            container.Chapter = db.Chapter.Find(container.ContentMediaPair.Content.ChapterId);
                        }
                        if (container.Chapter != null)
                        {
                            container.Book = db.Book.Find(container.Chapter.BookId);
                        }
                        break;
                }
            }

            return container;
        }

        public IEnumerable<TimelineItemContainer> GetGlobalItems(int num, int minId, int maxId)
        {
            using (var db = new MainContext())
            {
                var items = this.GetGlobalItems(db, num, minId, maxId);
                items.Load();
                return this.CheckTimelineItemContainerProperties(items.ToArray());
            }
        }

        public IQueryable<TimelineItemContainer> GetGlobalItems(MainContext db, int num, int minId, int maxId)
        {
            this.CheckParameters(num, minId, maxId);
            return db.TimelineItem.Where(i => i.Id <= maxId && i.Id >= minId)
                                  .Where(i => (i.ListRangeValue & (int)TimelineListRange.Global) > 0)
                                  .Take(num)
                                  .Join(db.User, i => i.UserId, u => u.Id, (i, u) => new TimelineItemContainer { TimelineItem = i, User = u, });
        }

        public IEnumerable<TimelineItemContainer> GetLocalItems(int num, int minId, int maxId)
        {
            using (var db = new MainContext())
            {
                var items = this.GetLocalItems(db, num, minId, maxId);
                items.Load();
                return this.CheckTimelineItemContainerProperties(items.ToArray());
            }
        }

        public IQueryable<TimelineItemContainer> GetLocalItems(MainContext db, int num, int minId, int maxId)
        {
            this.CheckParameters(num, minId, maxId);
            return this.GetItems(db, i => (i.ListRangeValue & (int)TimelineListRange.Local) > 0, num, minId, maxId);
        }

        private IEnumerable<TimelineItemContainer> CheckTimelineItemContainerProperties(IEnumerable<TimelineItemContainer> container)
        {
            foreach (var c in container)
            {
                if (c.ContentMediaPair.Content == null)
                {
                    c.ContentMediaPair = null;
                }
                else if (c.ContentMediaPair.Media != null)
                {
                    var mmed = new MediaModel
                    {
                        AuthData = this.AuthData,
                        Content = c.ContentMediaPair.Content,
                        Media = c.ContentMediaPair.Media,
                    };
                    mmed.SetMediaUri();
                }
            }
            return container;
        }

        private void CheckParameters(int num, int minId, int maxId)
        {
            if (minId > maxId)
            {
                throw new GunuccoException(new ApiMessage
                {
                    StatusCode = 400,
                    Message = "Invalid minimum and maximum ids.",
                });
            }
            if (num > 50)
            {
                throw new GunuccoException(new ApiMessage
                {
                    StatusCode = 400,
                    Message = "Number of timeline items too large.",
                });
            }
        }
    }
}
