﻿using Gunucco.Common;
using Gunucco.Entities;
using Gunucco.Models.Database;
using Gunucco.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Gunucco.Models.Entity
{
    public class ChapterModel
    {
        public AuthorizationData AuthData { get; set; }

        public Book Book { get; set; }

        public Chapter Chapter { get; set; }

        private bool isLoaded = false;

        public void Create()
        {
            var mbook = new BookModel
            {
                AuthData = this.AuthData,
                Book = this.Book,
            };

            this.CheckSentData();

            using (var db = new MainContext())
            {
                // can user create chapter in the book?
                mbook.CheckPermission(db);

                // check parent chapter permission
                if (this.Chapter.ParentId != null)
                {
                    this.CheckSetParent(db, this.Chapter.ParentId.Value);
                }

                // create chapter
                db.Chapter.Add(this.Chapter);

                db.SaveChanges();
            }
        }

        public void Load()
        {
            using (var db = new MainContext())
            {
                this.Load(db);
            }
        }

        public void Load(MainContext db)
        {
            if (this.isLoaded) return;
            this.isLoaded = true;

            var c = db.Chapter.Find(this.Chapter.Id);
            if (c == null)
            {
                throw new GunuccoException(new ApiMessage
                {
                    StatusCode = 404,
                    Message = "No such chapter id found.",
                });
            }

            this.Book = this.Book ?? new Book();
            if (this.Book.Id == default(int)) this.Book.Id = c.BookId;

            this.Chapter = c;
        }

        public void LoadWithPermissionCheck()
        {
            using (var db = new MainContext())
            {
                this.LoadWithPermissionCheck(db);
            }
        }

        public void LoadWithPermissionCheck(MainContext db)
        {
            this.Load(db);

            this.CheckLoadPermission(db);
        }

        public IEnumerable<Chapter> GetChildren()
        {
            using (var db = new MainContext())
            {
                var children = this.GetChildren(db);
                children.Load();
                return children.ToArray();
            }
        }

        public IQueryable<Chapter> GetChildren(MainContext db)
        {
            this.Load(db);
            return db.Chapter.Where(c => c.ParentId == this.Chapter.Id);
        }

        public IEnumerable<Chapter> GetChildrenWithPermissionCheck()
        {
            using (var db = new MainContext())
            {
                return this.GetChildrenWithPermissionCheck(db).ToArray();
            }
        }

        public IEnumerable<Chapter> GetChildrenWithPermissionCheck(MainContext db)
        {
            this.CheckLoadPermission(db);

            var children = this.GetChildren(db);
            foreach (var c in children)
            {
                var mchap = new ChapterModel
                {
                    AuthData = this.AuthData,
                    Chapter = c,
                    Book = this.Book,
                };

                bool isError = false;
                try
                {
                    mchap.CheckLoadPermission(db);
                }
                catch
                {
                    isError = true;
                }

                if (!isError)
                {
                    yield return c;
                }
            }
        }

        public IEnumerable<Content> GetContents()
        {
            using (var db = new MainContext())
            {
                var contents = this.GetContents(db);
                contents.Load();
                return contents.ToArray();
            }
        }

        public IQueryable<Content> GetContents(MainContext db)
        {
            this.Load(db);
            return db.Content.Where(c => c.ChapterId == this.Chapter.Id).OrderBy(c => c.Order);
        }

        public IEnumerable<ContentMediaPair> GetContentMediaPairs()
        {
            using (var db = new MainContext())
            {
                var pairs = this.GetContentMediaPairs(db);
                pairs.Load();
                return pairs.ToArray();
            }
        }

        public IQueryable<ContentMediaPair> GetContentMediaPairs(MainContext db)
        {
            this.Load(db);
            return db.Content.Where(c => c.ChapterId == this.Chapter.Id).OrderBy(c => c.Order)
                    .GroupJoin(db.Media, c => c.Id, m => m.ContentId, (c, ms) => new ContentMediaPair { Content = c, Media = ms.FirstOrDefault(), });
        }

        public IEnumerable<ContentMediaPair> GetContentMediaPairsWithPermissionCheck()
        {
            using (var db = new MainContext())
            {
                var results = this.GetContentMediaPairsWithPermissionCheck(db);
                results.Select(r => r.Media).Load();
                results.Select(r => r.Content).Load();

                var array = results.ToArray();
                foreach (var m in array.Where(r => r.Content.Type == ContentType.Image)
                                       .Select(r => new MediaModel
                {
                    AuthData = this.AuthData,
                    Content = r.Content,
                    Media = r.Media,
                }))
                {
                    m.SetMediaUri();
                }

                return array;
            }
        }

        public IQueryable<ContentMediaPair> GetContentMediaPairsWithPermissionCheck(MainContext db)
        {
            this.CheckLoadPermission(db);
            return this.GetContentMediaPairs(db);
        }

        public ApiMessage Save()
        {
            using (var db = new MainContext())
            {
                return this.Save(db);
            }
        }

        public ApiMessage Save(MainContext db)
        {
            this.isLoaded = true;
            this.CheckSentData();

            var current = new ChapterModel
            {
                AuthData = this.AuthData,
                Chapter = this.Chapter,
                Book = new Book { Id = this.Chapter.BookId, },
            };
            current.Load(db);

            // do user change un-changable property?
            if (current.Chapter.BookId != this.Chapter.BookId)
            {
                throw new GunuccoException(new ApiMessage
                {
                    StatusCode = 400,
                    Message = "Cannot change book. Book_id must be set current value (" + current.Chapter.BookId + ")",
                });
            }

            current.CheckPermission(db);

            // check new parent permission
            if (current.Chapter.ParentId != this.Chapter.ParentId && this.Chapter.ParentId != null)
            {
                this.CheckSetParent(db, this.Chapter.ParentId.Value);
            }

            // save
            var c = current.Chapter;
            db.Chapter.Attach(c);
            c.Name = this.Chapter.Name;
            c.ParentId = this.Chapter.ParentId;
            c.PublicRange = this.Chapter.PublicRange;
            c.Order = this.Chapter.Order;
            db.SaveChanges();

            return new ApiMessage
            {
                StatusCode = 200,
                Message = "Update chapter succeed.",
            };
        }

        public ApiMessage Delete()
        {
            using (var db = new MainContext())
            {
                return this.Delete(db);
            }
        }

        public ApiMessage Delete(MainContext db, bool isCheckPermission = true)
        {
            // get chapter data
            this.Load(db);

            // check permissions to delete
            var permissions = this.GetPermissions(db);
            if (isCheckPermission)
            {
                this.CheckPermission(db, permissions);
            }

            // get children
            var children = this.GetChildren(db);
            var childModels = new Collection<ChapterModel>();

            // check children permissions
            foreach (var child in children)
            {
                var mchap = new ChapterModel
                {
                    AuthData = this.AuthData,
                    Book = new Book { Id = this.Chapter.BookId, },
                    Chapter = child,
                };
                mchap.CheckPermission(db);

                childModels.Add(mchap);
            }

            // remove children
            foreach (var mchap in childModels)
            {
                mchap.Delete(db);
            }

            // remove data
            db.Chapter.Remove(this.Chapter);
            db.BookPermission.RemoveRange(permissions);

            db.SaveChanges();

            // remove contents
            var contents = this.GetContents(db);
            foreach (var c in contents)
            {
                var mcont = new ContentModel
                {
                    AuthData = this.AuthData,
                    Chapter = this.Chapter,
                    Content = c,
                };
                mcont.Delete(db, false);
            }

            return new ApiMessage
            {
                StatusCode = 200,
                Message = "Delete chapter succeed.",
            };
        }
        
        public IQueryable<BookPermission> GetPermissions(MainContext db)
        {
            return db.BookPermission.Where(p => p.UserId == this.AuthData.User.Id)
                                    .Where(p => p.TargetId == this.Chapter.Id &&
                                                p.TargetTypeValue == (short)TargetType.Chapter);
        }

        public void CheckPermission(MainContext db)
        {
            this.Load(db);

            var permissions = this.GetPermissions(db);
            this.CheckPermission(db, permissions);
        }

        private void CheckPermission(MainContext db, IQueryable<BookPermission> permissions)
        {
            if (!permissions.Any())
            {
                // check parent permission
                if (this.Chapter.ParentId.HasValue)
                {
                    var mparent = new ChapterModel
                    {
                        AuthData = this.AuthData,
                        Chapter = new Chapter { Id = this.Chapter.ParentId.Value, },
                        Book = this.Book,
                    };

                    // if parent has permissions, this method do nothing
                    mparent.CheckPermission(db);
                }

                else
                {
                    // check book permission if parent permissions not passed
                    var mbook = new BookModel
                    {
                        AuthData = this.AuthData,
                        Book = new Book { Id = this.Chapter.BookId, },
                    };
                    mbook.CheckPermission(db);
                }
            }
            // if user has permissions, this method do nothing
        }

        public void CheckLoadPermission(MainContext db)
        {
            this.Load(db);

            // except for All, user must login to get chapter
            if (this.Chapter.PublicRange != PublishRange.All)
            {
                if (!this.AuthData.IsAuthed)
                {
                    throw new GunuccoException(new ApiMessage
                    {
                        StatusCode = 403,
                        Message = "You must login to get permission of this chapter.",
                    });
                }
            }

            // this range allows users who has permission only
            if (this.Chapter.PublicRange == PublishRange.Private)
            {
                this.CheckPermission(db);
            }

            // check parent permission
            if (this.Chapter.ParentId.HasValue)
            {
                var mparent = new ChapterModel
                {
                    AuthData = this.AuthData,
                    Book = this.Book,
                    Chapter = new Chapter { Id = this.Chapter.ParentId.Value, },
                };
                mparent.Load(db);
                mparent.CheckLoadPermission(db);
            }
        }

        private void CheckSentData()
        {
            if (string.IsNullOrEmpty(this.Chapter.Name))
            {
                //throw new GunuccoException(new ApiMessage
                //{
                //    StatusCode = 400,
                //    Message = "No chapter name set.",
                //});
                this.Chapter.Name = string.Empty;
            }
            if (this.Chapter.Name.Length > 120 / 3)     // utf8
            {
                throw new GunuccoException(new ApiMessage
                {
                    StatusCode = 400,
                    Message = "Book name is too long.",
                });
            }

            if (this.Book != null)
            {
                this.Chapter.BookId = this.Book.Id;
            }
        }

        private void CheckSetParent(MainContext db, int parentId)
        {
            Chapter parent = null;
            var cchap = new ChapterModel
            {
                AuthData = this.AuthData,
                Chapter = new Chapter { Id = parentId, },
            };

            if (parentId == this.Chapter.Id)
            {
                throw new GunuccoException(new ApiMessage
                {
                    StatusCode = 400,
                    Message = "Cannot set self as parent chapter.",
                });
            }

            try
            {
                cchap.Load(db);
                parent = cchap.Chapter;
            }
            catch
            {
                throw new GunuccoException(new ApiMessage
                {
                    StatusCode = 404,
                    Message = "No such parent chapter id found.",
                });
            }

            this.Load(db);
            if (parent.BookId != this.Chapter.BookId)
            {
                throw new GunuccoException(new ApiMessage
                {
                    StatusCode = 400,
                    Message = "Cannot set parent in different book.",
                });
            }

            try
            {
                cchap.CheckPermission(db);
            }
            catch
            {
                throw new GunuccoException(new ApiMessage
                {
                    StatusCode = 403,
                    Message = "No permissions to set parent chapter.",
                });
            }

            // check parent loop
            if (parent.ParentId == this.Chapter.Id)
            {
                throw new GunuccoException(new ApiMessage
                {
                    StatusCode = 400,
                    Message = "Chapter parent is looping.",
                });
            }
            var currentChap = parent;
            while (currentChap.ParentId != null)
            {
                var pchap = new ChapterModel
                {
                    AuthData = this.AuthData,
                    Book = this.Book,
                    Chapter = new Chapter { Id = currentChap.ParentId.Value, },
                };
                pchap.Load(db);
                if (pchap.Chapter.ParentId == this.Chapter.Id)
                {
                    throw new GunuccoException(new ApiMessage
                    {
                        StatusCode = 400,
                        Message = "Chapter parent is looping.",
                    });
                }
                currentChap = pchap.Chapter;
            }
        }
    }
}
