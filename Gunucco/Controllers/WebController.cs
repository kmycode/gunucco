using Gunucco.Common;
using Gunucco.Entities;
using Gunucco.Entities.Helpers;
using Gunucco.Models;
using Gunucco.Models.Database;
using Gunucco.Models.Entities;
using Gunucco.Models.Entity;
using Gunucco.Models.Utils;
using Gunucco.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gunucco.Controllers
{
    [Route("/web")]
    public class WebController : Controller
    {
        public IActionResult Index()
        {
            return Redirect("/");
        }

        #region Sign up / in

        [HttpGet]
        [Route("signup")]
        public IActionResult SignUp()
        {
            try
            {
                UserModel.CheckNewSignupable();
            }
            catch (GunuccoException ex)
            {
                this.HttpContext.Response.StatusCode = ex.Error.StatusCode;
                return this.ShowMessage(ex.Error.Message);
            }

            return View();
        }

        [HttpPost]
        [Route("signup")]
        public IActionResult SignUp_Post(string text_id, string password, string password_again, string email, string email_again)
        {
            if (string.IsNullOrEmpty(text_id) || string.IsNullOrEmpty(password))
            {
                return this.ShowMessage("Text id or password isn't set.");
            }

            if (password != password_again)
            {
                return this.ShowMessage("Re-typed password is different.");
            }

            if (email != email_again)
            {
                return this.ShowMessage("Re-typed email is different.");
            }

            if (Config.IsEmailValidationNeed)
            {
                if (string.IsNullOrWhiteSpace(email) || !email.Contains('@') || email.First() == '@' || email.Last() == '@')
                {
                    return this.ShowMessage("Invalid e-mail address.");
                }
            }

            try
            {
                var muser = new UserModel();

                if (Config.IsEmailValidationNeed)
                {
                    muser.Create(text_id, password, email, false);
                    muser.SendValidationEmail(email);
                }
                else
                {
                    muser.Create(text_id, password, email);
                }
            }
            catch (GunuccoException ex)
            {
                this.HttpContext.Response.StatusCode = ex.Error.StatusCode;
                return this.ShowMessage(ex.Error.Message);
            }

            if (Config.IsEmailValidationNeed)
            {
                return this.ShowMessage("Email sent. Please check and activate email to complete sign up.", false);
            }
            else
            {
                return this.ShowMessage("Sign up completed. Welcome to Gunucco. Sign in to go MyPage and write books!", false);
            }
        }

        [HttpGet]
        [Route("signup/activate")]
        public IActionResult SignUp(int user_id, string key)
        {
            var muser = new UserModel
            {
                User = new User
                {
                    Id = user_id,
                },
            };
            try
            {
                muser.CheckActivateKeyValidation(key);
            }
            catch (GunuccoException ex)
            {
                return this.ShowMessage(ex.Error.Message);
            }

            return this.ShowMessage("Email validation completed. Welcome to Gunucco. Sign in to go MyPage and write books!", false);
        }

        [HttpGet]
        [Route("signin")]
        public IActionResult SignIn()
        {
            return View();
        }

        [HttpPost]
        [Route("signin")]
        public IActionResult SignIn_Post(string text_id, string password)
        {
            if (string.IsNullOrEmpty(text_id) || string.IsNullOrEmpty(password))
            {
                return this.ShowMessage("Text id or password isn't set.");
            }

            AuthorizationData authData = null;
            try
            {
                authData = Authentication.Authorize(text_id, password, Scope.WebClient);
            }
            catch (GunuccoException ex)
            {
                this.HttpContext.Response.StatusCode = ex.Error.StatusCode;
                return this.ShowMessage(ex.Error.Message);
            }

            return this.MyPage(authData);
        }

        #endregion

        #region Oauth

        [HttpGet]
        [Route("oauth")]
        public IActionResult OauthRequest(string code)
        {
            Scope scope = Scope.None;
            try
            {
                scope = Authentication.GetOauthCodeScopeForOauthRequest(code);
            }
            catch (GunuccoException ex)
            {
                this.HttpContext.Response.StatusCode = ex.Error.StatusCode;
                return this.ShowMessage(ex.Error.Message);
            }

            var vm = new OauthViewViewModel
            {
                Code = code,
                Scope = scope,
            };

            return View(vm);
        }

        [HttpPost]
        [Route("oauth/done")]
        public IActionResult OauthAccept(string code, string text_id, string password)
        {
            try
            {
                var authData = Authentication.Authorize(text_id, password, Scope.WebClient);
                Authentication.AuthorizeWithOauth(code, authData);
            }
            catch (GunuccoException ex)
            {
                this.HttpContext.Response.StatusCode = ex.Error.StatusCode;
                return this.ShowMessage(ex.Error.Message);
            }

            return this.ShowMessage("Oauth request is done. Back client application to continue.", false);
        }

        #endregion

        #region View

        [HttpGet]
        [Route("user/{text_id}")]
        public IActionResult ViewUser(string text_id)
        {
            var muser = new UserModel
            {
                User = new User { TextId = text_id, },
            };
            var mbook = new BookModel();

            IEnumerable<Book> books = null;

            int id = 0;
            try
            {
                muser.LoadWithTextId();
                id = muser.User.Id;
                books = mbook.GetUserBooks(id);
            }
            catch (GunuccoException ex)
            {
                return this.ShowMessage(ex.Error.Message);
            }

            var vm = new UserViewViewModel
            {
                User = muser.User,
                Books = books,
            };

            return View("View_user", vm);
        }

        [HttpGet]
        [Route("book/{id}")]
        public IActionResult ViewBook(int id)
        {
            var mbook = new BookModel
            {
                Book = new Book
                {
                    Id = id,
                },
            };
            var muser = new UserModel();

            IEnumerable<TreeEntity<Chapter>> chapters = null;

            try
            {
                mbook.Load();
                var owners = mbook.GetOwners();

                muser.User = UserModel.FromIdOrAnonymous(owners.FirstOrDefault().UserId).User;
                muser.Load();

                chapters = TreeEntity<Chapter>.FromEntities(mbook.GetChaptersWithPermissionCheck(), c => c.ParentId);
            }
            catch (GunuccoException ex)
            {
                return this.ShowMessage(ex.Error.Message);
            }

            var vm = new BookViewViewModel
            {
                User = muser.User,
                Book = mbook.Book,
                Chapters = chapters,
            };

            return View("View_book", vm);
        }

        [HttpGet]
        [Route("book/{book_id}/chapter/{id}")]
        public IActionResult ViewChapter(int book_id, int id)
        {
            var mbook = new BookModel
            {
                Book = new Book
                {
                    Id = book_id,
                },
            };
            var mchap = new ChapterModel
            {
                Chapter = new Chapter
                {
                    Id = id,
                },
            };

            IList<Chapter> chapters = null;
            IEnumerable<ContentMediaPair> contents = null;
            Chapter chapter = null;

            try
            {
                mbook.Load();

                chapters = TreeEntity<Chapter>.FromEntities(mbook.GetChapters(), c => c.ParentId).Select(c => c.Item).ToList();
                chapter = chapters.SingleOrDefault(c => c.Id == id);
                if (chapter == null)
                {
                    throw new GunuccoException(new ApiMessage
                    {
                        StatusCode = 404,
                        Message = "No chapter id found.",
                    });
                }

                contents = mchap.GetContentMediaPairsWithPermissionCheck();
            }
            catch (GunuccoException ex)
            {
                return this.ShowMessage(ex.Error.Message);
            }

            var vm = new ChapterViewViewModel
            {
                Book = mbook.Book,
                Chapter = chapter,
                Contents = contents,
                PrevChapter = chapters.FindPrev(c => c.Id == chapter.Id),
                NextChapter = chapters.FindNext(c => c.Id == chapter.Id),
            };

            return View("View_chapter", vm);
        }

        #endregion

        #region MyPage

        [HttpPost]
        [Route("mypage")]
        public IActionResult MyPage(string auth_token)
        {
            return this.MyPage_Common(auth_token);
        }

        [HttpPost]
        private IActionResult MyPage(AuthorizationData authData)
        {
            return this.MyPage_Common(authData);
        }

        [HttpPost]
        [Route("mypage/book/new")]
        public IActionResult MyPage_CreateNewBook(string auth_token, string book_name)
        {
            return this.MyPage_Common(auth_token, authData =>
            {
                var mbook = new BookModel
                {
                    AuthData = authData,
                    Book = new Book
                    {
                        Name = book_name,
                    },
                };

                mbook.Create();
            });
        }
        
        private IActionResult MyPage_Common(string auth_token, Action<AuthorizationData> action = null)
        {
            AuthorizationData authData = null;
            try
            {
                authData = Authentication.Authorize(auth_token);
            }
            catch (GunuccoException ex)
            {
                return this.ShowMessage(ex.Error.Message);
            }
            return this.MyPage_Common(authData, action);
        }

        private IActionResult MyPage_Common(AuthorizationData authData, Action<AuthorizationData> action = null)
        {
            MessageViewModel mes = new MessageViewModel();
            try
            {
                action?.Invoke(authData);
            }
            catch (GunuccoException ex)
            {
                mes.HasMessage = true;
                mes.IsError = true;
                mes.AddMessage(ex.Error.Message);
            }

            var mbook = new BookModel
            {
                AuthData = authData,
            };
            var books = mbook.GetUserBooks(authData.User.Id);

            var vm = new MyPageTopViewModel
            {
                AuthData = authData,
                Books = books,
                Message = mes,
            };

            return View("MyPage", vm);
        }

        [HttpPost]
        [Route("mypage/book")]
        public IActionResult MyBook(string auth_token, int book_id)
        {
            return this.MyBook_Common(auth_token, book_id);
        }

        private IActionResult MyBook(AuthorizationData authData, int book_id)
        {
            return this.MyBook_Common(authData, book_id);
        }

        [HttpPost]
        [Route("mypage/chapter/new")]
        public IActionResult MyBook_CreateChapter(string auth_token, int book_id, string chapter_name)
        {
            return this.MyBook_Common(auth_token, book_id, bm =>
            {
                var mchap = new ChapterModel
                {
                    AuthData = bm.AuthData,
                    Book = bm.Book,
                    Chapter = new Chapter { Name = chapter_name, },
                };
                mchap.Create();
            });
        }

        [HttpPost]
        [Route("mypage/chapter/reorder")]
        public IActionResult MyBook_Reorder(string auth_token, int book_id, int chapter_id, int? chapter_order)
        {
            return this.MyBook_Common(auth_token, book_id, bm =>
            {
                if (chapter_order == null)
                {
                    throw new GunuccoException(new ApiMessage
                    {
                        Message = "Chapter order isn't set.",
                        StatusCode = 400,
                    });
                }

                var mchap = new ChapterModel
                {
                    AuthData = bm.AuthData,
                    Book = bm.Book,
                    Chapter = new Chapter { Id = chapter_id, },
                };
                using (var db = new MainContext())
                {
                    mchap.Load(db);
                    mchap.Chapter.Order = chapter_order.Value;
                    mchap.Save(db);
                }
            });
        }

        private IActionResult MyBook_Common(string auth_token, int book_id, Action<BookModel> action = null, bool isHeaderMessage = true)
        {
            AuthorizationData authData = null;
            try
            {
                authData = Authentication.Authorize(auth_token);
            }
            catch (GunuccoException ex)
            {
                return this.ShowMessage(ex.Error.Message);
            }

            return this.MyBook_Common(authData, book_id, action, isHeaderMessage);
        }

        private IActionResult MyBook_Common(AuthorizationData authData, int book_id, Action<BookModel> action = null, bool isHeaderMessage = true)
        {
            var mbook = new BookModel
            {
                AuthData = authData,
                Book = new Book
                {
                    Id = book_id,
                },
            };
            try
            {
                mbook.Load();
            }
            catch (GunuccoException ex)
            {
                return this.ShowMessage(ex.Error.Message);
            }

            // do custom action
            MessageViewModel mes = new MessageViewModel();
            try
            {
                action?.Invoke(mbook);
            }
            catch (GunuccoException ex)
            {
                if (isHeaderMessage)
                {
                    mes.HasMessage = true;
                    mes.IsError = true;
                    mes.AddMessage(ex.Error.Message);
                }
                else
                {
                    return this.ShowMessage(ex.Error.Message);
                }
            }

            // load chapters
            IEnumerable<TreeEntity<Chapter>> chapters = null;
            try
            {
                var raw = mbook.GetChaptersWithPermissionCheck();
                chapters = TreeEntity<Chapter>.FromEntities(raw, c => c.ParentId);
            }
            catch (GunuccoException ex)
            {
                mes.HasMessage = true;
                mes.IsError = true;
                mes.AddMessage(ex.Error.Message);
                chapters = Enumerable.Empty<TreeEntity<Chapter>>();
            }

            var vm = new MyPageBookViewModel
            {
                AuthData = authData,
                Message = mes,
                Book = mbook.Book,
                Chapters = chapters,
            };

            return View("MyPage_book", vm);
        }

        [HttpPost]
        [Route("mypage/chapter")]
        public IActionResult MyChapter(string auth_token, int book_id, int chapter_id)
        {
            return this.MyChapter_Common(auth_token, book_id, chapter_id);
        }

        [HttpPost]
        [Route("mypage/content/text/new")]
        public IActionResult MyChapter_CreateTextContent(string auth_token, int book_id, int chapter_id)
        {
            return this.MyChapter_Common(auth_token, book_id, chapter_id, cm =>
            {
                var mcont = new ContentModel
                {
                    AuthData = cm.AuthData,
                    Chapter = cm.Chapter,
                    Content = new Content
                    {
                        ChapterId = cm.Chapter.Id,
                        Text = "",
                        Type = ContentType.Text,
                    },
                };
                mcont.Create();
            });
        }

        [HttpPost]
        [Route("mypage/content/image/new")]
        public IActionResult MyChapter_CreateImageContent(string auth_token, int book_id, int chapter_id, IList<IFormFile> content_images)
        {
            return this.MyChapter_Common(auth_token, book_id, chapter_id, cm =>
            {
                int failedCount = 0;

                foreach (var image in content_images)
                {
                    string base64 = null;
                    MediaExtension extension = MediaExtension.Outside;
                    try
                    {
                        if (image.FileName.EndsWith(".png"))
                        {
                            extension = MediaExtension.Png;
                        }
                        else if (image.FileName.EndsWith(".jpeg") || image.FileName.EndsWith(".jpg"))
                        {
                            extension = MediaExtension.Jpeg;
                        }
                        else if (image.FileName.EndsWith(".gif"))
                        {
                            extension = MediaExtension.Gif;
                        }
                        else
                        {
                            throw new Exception();
                        }

                        base64 = image.ToBase64();
                    }
                    catch
                    {
                        failedCount++;
                    }

                    var mcont = new ContentModel
                    {
                        AuthData = cm.AuthData,
                        Chapter = cm.Chapter,
                        Content = new Content
                        {
                            ChapterId = cm.Chapter.Id,
                            Text = "",
                            Type = ContentType.Image,
                        },
                        Media = new Media
                        {
                            Type = MediaType.Image,
                            Source = MediaSource.Self,
                            Extension = extension,
                            MediaData = base64,
                        },
                    };
                    mcont.Create();
                }

                if (failedCount > 0)
                {
                    throw new GunuccoException(new ApiMessage
                    {
                        StatusCode = 400,
                        Message = "Some images failed: " + failedCount,
                    });
                }
            });
        }

        [HttpPost]
        [Route("mypage/content/reorder")]
        public IActionResult MyChapter_Reorder(string auth_token, int book_id, int chapter_id, int content_id, int? content_order)
        {
            return this.MyChapter_Common(auth_token, book_id, chapter_id, cm =>
            {
                if (content_order == null)
                {
                    throw new GunuccoException(new ApiMessage
                    {
                        Message = "Content order isn't set.",
                        StatusCode = 400,
                    });
                }

                var mcont = new ContentModel
                {
                    AuthData = cm.AuthData,
                    Chapter = cm.Chapter,
                    Content = new Content
                    {
                        Id = content_id,
                    },
                };
                using (var db = new MainContext())
                {
                    mcont.Load(db);
                    mcont.Content.Order = content_order.Value;
                    mcont.Save(db);
                }
            });
        }

        [HttpPost]
        [Route("mypage/content/text/edit")]
        public IActionResult MyChapter_EditTextContent(string auth_token, int book_id, int chapter_id, int content_id, string content_text, string is_delete)
        {
            return this.MyChapter_Common(auth_token, book_id, chapter_id, cm =>
            {
                var mcont = new ContentModel
                {
                    AuthData = cm.AuthData,
                    Chapter = cm.Chapter,
                    Content = new Content
                    {
                        Id = content_id,
                    },
                };
                if (string.IsNullOrEmpty(is_delete))
                {
                    using (var db = new MainContext())
                    {
                        mcont.Load(db);
                        mcont.Content.Text = content_text;
                        mcont.Save(db);
                    }
                }
                else
                {
                    mcont.Delete();
                }
            });
        }

        [HttpPost]
        [Route("mypage/chapter/edit")]
        public IActionResult MyChapter_Edit(string auth_token, int book_id, int chapter_id, string chapter_name, string chapter_publish_range, string is_delete)
        {
            AuthorizationData authData = null;
            var result = this.MyChapter_Common(auth_token, book_id, chapter_id, cm =>
            {
                PublishRange range = PublishRange.All;
                if (chapter_publish_range == "all") { }
                else if (chapter_publish_range == "private")
                {
                    range = PublishRange.Private;
                }
                else
                {
                    throw new GunuccoException(new ApiMessage
                    {
                        StatusCode = 400,
                        Message = "Publish range isn't set.",
                    });
                }

                if (string.IsNullOrEmpty(is_delete))
                {
                    cm.Chapter.Name = chapter_name;
                    cm.Chapter.PublicRange = range;
                    cm.Save();
                }
                else
                {
                    cm.Delete();
                }

                authData = cm.AuthData;
            });

            if (string.IsNullOrEmpty(is_delete))
            {
                return result;
            }
            else
            {
                return this.MyBook(authData, book_id);
            }
        }

        private IActionResult MyChapter_Common(string auth_token, int book_id, int chapter_id, Action<ChapterModel> action = null, bool isHeaderMessage = true)
        {
            AuthorizationData authData = null;
            try
            {
                authData = Authentication.Authorize(auth_token);
            }
            catch (GunuccoException ex)
            {
                return this.ShowMessage(ex.Error.Message);
            }

            var mbook = new BookModel
            {
                AuthData = authData,
                Book = new Book
                {
                    Id = book_id,
                },
            };
            var mchap = new ChapterModel
            {
                AuthData = authData,
                Book = new Book
                {
                    Id = book_id,
                },
                Chapter = new Chapter
                {
                    BookId = book_id,
                    Id = chapter_id,
                },
            };
            try
            {
                mbook.Load();
                mchap.Load();
            }
            catch (GunuccoException ex)
            {
                return this.ShowMessage(ex.Error.Message);
            }

            // do custom action
            MessageViewModel mes = new MessageViewModel();
            try
            {
                action?.Invoke(mchap);
            }
            catch (GunuccoException ex)
            {
                if (isHeaderMessage)
                {
                    mes.HasMessage = true;
                    mes.IsError = true;
                    mes.AddMessage(ex.Error.Message);
                }
                else
                {
                    return this.ShowMessage(ex.Error.Message);
                }
            }

            // load contents
            IEnumerable<ContentMediaPair> contents = null;
            try
            {
                contents = mchap.GetContentMediaPairsWithPermissionCheck();
            }
            catch (GunuccoException ex)
            {
                mes.HasMessage = true;
                mes.IsError = true;
                mes.AddMessage(ex.Error.Message);
                contents = Enumerable.Empty<ContentMediaPair>();
            }

            var vm = new MyPageChapterViewModel
            {
                AuthData = authData,
                Message = mes,
                Book = mbook.Book,
                Chapter = mchap.Chapter,
                Contents = contents,
            };

            return View("MyPage_chapter", vm);
        }

        #endregion

        private IActionResult ShowMessage(string mes, bool isError = true)
        {
            ViewBag.Message = mes;
            ViewBag.IsError = isError;
            return View("Message");
        }
    }
}
