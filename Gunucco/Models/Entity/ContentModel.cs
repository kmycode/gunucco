using Gunucco.Common;
using Gunucco.Entities;
using Gunucco.Models.Database;
using Gunucco.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gunucco.Models.Entity
{
    public class ContentModel
    {
        private const string MediaDirPath = "/media/";

        public AuthenticationData AuthData { get; set; }

        public Chapter Chapter { get; set; }

        public Content Content { get; set; }

        public Media Media { get; set; }

        public ContentMediaPair Pair => new ContentMediaPair
        {
            Content = this.Content,
            Media = this.Media,
        };

        private bool isLoaded = false;
        
        public void Create()
        {
            this.CheckSentData();

            using (var db = new MainContext())
            {
                // can user create contents in the chapter?
                this.CheckPermission(db);

                // create content
                this.Content.Created = this.Content.LastModified = DateTime.Now;
                this.Content.ChapterId = this.Chapter.Id;
                db.Content.Add(this.Content);
                db.SaveChanges();

                if (this.Content.Type == ContentType.Image)
                {
                    this.Media.ContentId = this.Content.Id;

                    // save media file
                    var mmedia = new MediaModel
                    {
                        AuthData = this.AuthData,
                        Content = this.Content,
                        Media = this.Media,
                    };
                    this.Media.FilePath = "Dummy";
                    db.Media.Add(this.Media);
                    db.SaveChanges();       // get media id

                    mmedia.SaveMediaAsFile();
                    this.SetMediaUri();
                    db.SaveChanges();
                }
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

            var c = db.Content.Find(this.Content.Id);
            if (c == null)
            {
                throw new GunuccoException(new ApiMessage
                {
                    StatusCode = 404,
                    Message = "No such content id found.",
                });
            }
            this.Content = c;

            this.LoadMedia(db);

            this.Chapter = this.Chapter ?? new Chapter();
            this.Chapter.Id = c.ChapterId;
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

            var cchap = new ChapterModel
            {
                AuthData = this.AuthData,
                Chapter = this.Chapter,
            };
            cchap.CheckLoadPermission(db);
        }

        public static ContentMediaPair GetPairFromMediaPath(AuthenticationData data, string path)
        {
            using (var db = new MainContext())
            {
                var media = db.Media.SingleOrDefault(m => m.FilePath != null && m.FilePath.EndsWith(path)) ??
                            throw new GunuccoException(new ApiMessage
                            {
                                StatusCode = 404,
                                Message = "No media file found.",
                            });
                var content = db.Content.SingleOrDefault(c => c.Id == media.ContentId) ??
                            throw new GunuccoException(new ApiMessage
                            {
                                StatusCode = 404,
                                Message = "No content data found.",
                            });

                var mmedia = new MediaModel
                {
                    AuthData = data,
                    Content = content,
                    Media = media,
                };
                mmedia.LoadMediaFromFile();

                var mcont = new ContentModel
                {
                    AuthData = data,
                    Content = content,
                    Media = media,
                    Chapter = new Chapter { Id = content.ChapterId, },
                };
                mcont.isLoaded = true;
                mcont.CheckLoadPermission(db);

                return new ContentMediaPair
                {
                    Content = content,
                    Media = media,
                };
            }
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

            var current = new ContentModel
            {
                AuthData = this.AuthData,
                Chapter = this.Chapter,
                Content = this.Content,
                Media = this.Media,
            };
            current.Load(db);

            // do user change media property?
            if (current.Content.Type != this.Content.Type)
            {
                throw new GunuccoException(new ApiMessage
                {
                    StatusCode = 400,
                    Message = "Cannot change content type. Type must be set current value (" + current.Content.TypeValue + ")",
                });
            }

            current.CheckPermission(db);

            // check new chapter permission
            if (current.Content.ChapterId != this.Content.ChapterId)
            {
                this.CheckSetChapter(db, this.Content.ChapterId);
            }

            // save
            var c = current.Content;
            db.Content.Attach(c);
            c.ChapterId = this.Content.ChapterId;
            c.Text = this.Content.Text;
            c.LastModified = DateTime.Now;
            c.Order = this.Content.Order;
            db.SaveChanges();

            return new ApiMessage
            {
                StatusCode = 200,
                Message = "Update content succeed.",
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
            // get content data
            this.Load(db);

            // check permissions to delete
            if (isCheckPermission)
            {
                this.CheckPermission(db);
            }

            // remove data
            db.Content.Remove(this.Content);
            if (this.Content.Type == ContentType.Image)
            {
                var mmedia = new MediaModel
                {
                    AuthData = this.AuthData,
                    Media = this.Media,
                    Content = this.Content,
                };
                mmedia.DeleteMediaFile();
                db.Media.Attach(this.Media);
                db.Media.Remove(this.Media);
            }

            db.SaveChanges();

            return new ApiMessage
            {
                StatusCode = 200,
                Message = "Delete content succeed.",
            };
        }
        
        public void LoadMedia(MainContext db)
        {
            if (this.Content.Type == ContentType.Image)
            {
                this.Media = db.Media.FirstOrDefault(m => m.ContentId == this.Content.Id);
                if (this.Media == null)
                {
                    throw new GunuccoException(new ApiMessage
                    {
                        StatusCode = 500,
                        Message = "Unable to load media data.",
                    });
                }

                // set download uri
                this.SetMediaUri();
            }

            if (this.Media == null)
            {
                this.Media = new Media { ContentId = this.Content.Id, };
            }
        }

        public void CheckPermission(MainContext db)
        {
            var mchap = new ChapterModel
            {
                AuthData = this.AuthData,
                Chapter = this.Chapter,
                Book = new Book { Id = this.Chapter.BookId, },
            };
            mchap.CheckPermission(db);
        }

        public void CheckLoadPermission(MainContext db)
        {
            var mchap = new ChapterModel
            {
                AuthData = this.AuthData,
                Chapter = this.Chapter,
                Book = new Book { Id = this.Chapter.BookId, },
            };
            mchap.CheckLoadPermission(db);
        }

        private void CheckSentData()
        {
            if (this.Content.Type == ContentType.Text)
            {
                if (this.Content.Text == null)
                {
                    this.Content.Text = string.Empty;
                }
                if (this.Content.Text.Length > 60000 / 3)   // utf8: 3 bytes per 1 mbchar
                {
                    throw new GunuccoException(new ApiMessage
                    {
                        StatusCode = 400,
                        Message = "Content text is too long.",
                    });
                }
            }
            else if (this.Content.Type == ContentType.Image)
            {
                if (this.Media.Source == MediaSource.Self)
                {
                    if (this.Media.MediaData == null)
                    {
                        throw new GunuccoException(new ApiMessage
                        {
                            StatusCode = 400,
                            Message = "Media data is not set.",
                        });
                    }
                    try
                    {
                        this.Media.MediaDataRow = Convert.FromBase64String(this.Media.MediaData);
                    }
                    catch
                    {
                        throw new GunuccoException(new ApiMessage
                        {
                            StatusCode = 400,
                            Message = "Media data is not base64 format.",
                        });
                    }
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(this.Media.FilePath))
                    {
                        throw new GunuccoException(new ApiMessage
                        {
                            StatusCode = 400,
                            Message = "Media outside uri is not set.",
                        });
                    }
                    if (!this.Media.FilePath.StartsWith("http://") && !this.Media.FilePath.StartsWith("https://"))
                    {
                        throw new GunuccoException(new ApiMessage
                        {
                            StatusCode = 400,
                            Message = "Media outside uri has invalid scheme. http or https are only arrowed.",
                        });
                    }
                }

                this.Content.Text = string.Empty;
            }
        }

        private void CheckSetChapter(MainContext db, int chapterId)
        {
            Chapter chapter = null;
            var cchap = new ChapterModel
            {
                AuthData = this.AuthData,
                Chapter = new Chapter { Id = chapterId, },
            };

            try
            {
                cchap.Load(db);
                chapter = cchap.Chapter;
            }
            catch
            {
                throw new GunuccoException(new ApiMessage
                {
                    StatusCode = 404,
                    Message = "No such chapter id found.",
                });
            }

            this.Load(db);

            try
            {
                cchap.CheckPermission(db);
            }
            catch
            {
                throw new GunuccoException(new ApiMessage
                {
                    StatusCode = 403,
                    Message = "No permissions to set content to chapter.",
                });
            }
        }

        private void SetMediaUri()
        {
            if (this.Media.Source == MediaSource.Self)
            {
                this.Media.Uri = Config.ServerPath + "/api/v1/download" + this.Media.FilePath;
            }
            else
            {
                this.Media.Uri = this.Media.FilePath;
            }
        }
    }
}
