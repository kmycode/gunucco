using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Gunucco.Models;
using Gunucco.Entities;
using Microsoft.AspNetCore.Mvc.Filters;
using Gunucco.Common;
using Gunucco.Filters;
using Gunucco.Models.Entity;
using Gunucco.Models.Entities;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;
using Gunucco.Models.Utils;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Gunucco.Controllers
{
    [Route("/api/v1")]
    [ArrowCrossDomain]
    [GunuccoExceptionFilter]
    [RouteTraceFilter]
    public class Api1Controller : Controller
    {
        public AuthorizationData AuthData { get; set; }

        public IActionResult Index()
        {
            throw new GunuccoException(new ApiMessage
            {
                StatusCode = 501,
                Message = "Not implemented action.",
            });
        }

        #region Server

        [HttpGet]
        [Route("server/version")]
        public IActionResult GetServerVersion()
        {
            return Json(new ApiMessage
            {
                StatusCode = 200,
                Message = Config.ServerVersion,
            });
        }

        #endregion

        #region Authorization Management

        [HttpGet]
        [Route("auth/list")]
        [AuthorizeFilter(Scope = Scope.ReadUserIdentity)]
        public IActionResult GetAuthorizations()
        {
            var msess = new SessionModel
            {
                AuthData = this.AuthData,
            };
            var data = msess.GetData();

            return Json(data);
        }

        [HttpDelete]
        [Route("auth/delete")]
        [AuthorizeFilter(Scope = Scope.WriteUserDangerousIdentity)]
        public IActionResult DeleteAuthorization(string id_hash)
        {
            var msess = new SessionModel
            {
                AuthData = this.AuthData,
            };
            var mes = msess.CancelAuthorization(id_hash);

            return Json(mes);
        }

        #endregion

        #region User

        [HttpPost]
        [Route("user/create")]
        [DebugModeOnlyFilter]
        public IActionResult CreateUser(string id, string password)
        {
            var muser = new UserModel();
            muser.Create(id, password);

            return Json(muser.AuthData.User);
        }

        [HttpGet]
        [Route("user/{id}")]
        [AuthorizeFilter(IsCheckAuthorizable = false, Scope = Scope.Read)]
        public IActionResult GetUser(int id)
        {
            var muser = new UserModel
            {
                AuthData = this.AuthData,
                User = new User
                {
                    Id = id,
                },
            };
            muser.Load();

            return Json(muser.User);
        }

        [HttpGet]
        [Route("user/{id}/text")]
        [AuthorizeFilter(IsCheckAuthorizable = false, Scope = Scope.Read)]
        public IActionResult GetUserWithTextId(string id)
        {
            var muser = new UserModel
            {
                AuthData = this.AuthData,
                User = new User
                {
                    TextId = id,
                },
            };
            muser.LoadWithTextId();

            return Json(muser.User);
        }

        [HttpDelete]
        [Route("user/delete")]
#if !DEBUG && !UNITTEST
        [AuthorizeFilter(Scope = Scope.WriteUserDangerousIdentity)]
#else
        [AuthorizeFilter(Scope = Scope.None)]
#endif
        public IActionResult DeleteUser()
        {
            var muser = new UserModel
            {
                AuthData = this.AuthData,
            };
            var mes = muser.Delete();

            return Json(mes);
        }

        [HttpPost]
        [Route("user/login/idandpassword")]
        public IActionResult LoginWithIdAndPassword(string id, string password)
        {
            var data = Authentication.Authorize(id, password);

            return Json(data.AuthToken);
        }

        [HttpGet]
        [Route("user/login/oauthcode/create")]
        public IActionResult GetOauthCode(int scope)
        {
            var code = Authentication.CreateOauthCode((Scope)scope);

            return Json(code);
        }

        [HttpPost]
        [Route("user/login/oauthcode")]
        public IActionResult LoginWithOauthCode(string code)
        {
            var token = Authentication.GetTokenWithAuthCode(code);

            return Json(token);
        }

        [HttpGet]
        [Route("user/{id}/books")]
        [AuthorizeFilter(IsCheckAuthorizable = false, Scope = Scope.Read)]
        public IActionResult GetUserBooks(int id)
        {
            var mbook = new BookModel
            {
                AuthData = this.AuthData,
            };
            var books = mbook.GetUserBooks(id);

            return Json(books);
        }

        #endregion

        #region Book

        [HttpPost]
        [Route("book/create")]
        [AuthorizeFilter(Scope = Scope.Write)]
        public IActionResult CreateBook(string name)
        {
            var mbook = new BookModel
            {
                AuthData = this.AuthData,
                Book = new Book
                {
                    Name = name,
                },
            };
            mbook.Create();

            return Json(mbook.Book);
        }

        [HttpGet]
        [Route("book/{id}")]
        [AuthorizeFilter(IsCheckAuthorizable = false, Scope = Scope.Read)]
        public IActionResult GetBook(int id)
        {
            var mbook = new BookModel
            {
                AuthData = this.AuthData,
                Book = new Book
                {
                    Id = id,
                },
            };
            mbook.Load();

            return Json(mbook.Book);
        }

        [HttpGet]
        [Route("book/{id}/chapters")]
        [AuthorizeFilter(IsCheckAuthorizable = false, Scope = Scope.Read)]
        public IActionResult GetChapters(int id)
        {
            var mbook = new BookModel
            {
                AuthData = this.AuthData,
                Book = new Book
                {
                    Id = id,
                },
            };
            var chapters = mbook.GetChaptersWithPermissionCheck();

            return Json(chapters);
        }

        [HttpGet]
        [Route("book/{id}/chapters/root")]
        [AuthorizeFilter(IsCheckAuthorizable = false, Scope = Scope.Read)]
        public IActionResult GetRootChapters(int id)
        {
            var mbook = new BookModel
            {
                AuthData = this.AuthData,
                Book = new Book
                {
                    Id = id,
                },
            };
            var chapters = mbook.GetRootChaptersWithPermissionCheck();

            return Json(chapters);
        }

        [HttpPut]
        [Route("book/update")]
        [AuthorizeFilter(Scope = Scope.Write)]
        public IActionResult UpdateBook(string book)
        {
            Book b = this.LoadJson<Book>(book);

            var mbook = new BookModel
            {
                AuthData = this.AuthData,
                Book = b,
            };
            var mes = mbook.Save();

            return Json(mes);
        }

        [HttpDelete]
        [Route("book/delete")]
        [AuthorizeFilter(Scope = Scope.Write)]
        public IActionResult DeleteBook(int id)
        {
            var mbook = new BookModel
            {
                AuthData = this.AuthData,
                Book = new Book
                {
                    Id = id,
                },
            };
            var mes = mbook.Delete();

            return Json(mes);
        }

        #endregion

        #region Chapter

        [HttpPost]
        [Route("chapter/create")]
        [AuthorizeFilter(Scope = Scope.Write)]
        public IActionResult CreateChapter(string name, int book_id, int? parent_chapter_id)
        {
            var mchap = new ChapterModel
            {
                AuthData = this.AuthData,
                Book = new Book
                {
                    Id = book_id,
                },
                Chapter = new Chapter
                {
                    Name = name,
                    BookId = book_id,
                    ParentId = parent_chapter_id,
                    PublicRange = PublishRange.Private,
                },
            };
            mchap.Create();

            return Json(mchap.Chapter);
        }

        [HttpGet]
        [Route("chapter/{id}")]
        [AuthorizeFilter(IsCheckAuthorizable = false, Scope = Scope.Read)]
        public IActionResult GetChapter(int id)
        {
            var mchap = new ChapterModel
            {
                AuthData = this.AuthData,
                Chapter = new Chapter
                {
                    Id = id,
                },
            };
            mchap.LoadWithPermissionCheck();

            return Json(mchap.Chapter);
        }

        [HttpGet]
        [Route("chapter/{id}/children")]
        [AuthorizeFilter(IsCheckAuthorizable = false, Scope = Scope.Read)]
        public IActionResult GetChildrenChapters(int id)
        {
            var mchap = new ChapterModel
            {
                AuthData = this.AuthData,
                Chapter = new Chapter
                {
                    Id = id,
                },
            };
            var children = mchap.GetChildrenWithPermissionCheck();

            return Json(children);
        }

        [HttpGet]
        [Route("chapter/{id}/contents")]
        [AuthorizeFilter(IsCheckAuthorizable = false, Scope = Scope.Read)]
        public IActionResult GetChapterContents(int id)
        {
            var mchap = new ChapterModel
            {
                AuthData = this.AuthData,
                Chapter = new Chapter
                {
                    Id = id,
                },
            };
            var contents = mchap.GetContentMediaPairsWithPermissionCheck();

            return Json(contents);
        }

        [HttpPut]
        [Route("chapter/update")]
        [AuthorizeFilter(Scope = Scope.Write)]
        public IActionResult UpdateChapter(string chapter)
        {
            Chapter chap = this.LoadJson<Chapter>(chapter);

            var mchap = new ChapterModel
            {
                AuthData = this.AuthData,
                Chapter = chap,
            };
            var mes = mchap.Save();

            return Json(mes);
        }

        [HttpDelete]
        [Route("chapter/delete")]
        [AuthorizeFilter(Scope = Scope.Write)]
        public IActionResult DeleteChapter(int id)
        {
            var mchap = new ChapterModel
            {
                AuthData = this.AuthData,
                Chapter = new Chapter { Id = id, },
            };
            var mes = mchap.Delete();
            
            return Json(mes);
        }

        #endregion

        #region Content and Media

        [HttpPost]
        [Route("content/create/text")]
        [AuthorizeFilter(Scope = Scope.Write)]
        public IActionResult CreateTextContent(int chapter_id, string text)
        {
            var mcont = new ContentModel
            {
                AuthData = this.AuthData,
                Chapter = new Chapter { Id = chapter_id, },
                Content = new Content
                {
                    Type = ContentType.Text,
                    Text = text,
                },
            };
            mcont.Create();

            return Json(mcont.Pair);
        }

        [HttpPost]
        [Route("content/create/html")]
        [AuthorizeFilter(Scope = Scope.Write)]
        public IActionResult CreateHtmlContent(int chapter_id, string html)
        {
            var mcont = new ContentModel
            {
                AuthData = this.AuthData,
                Chapter = new Chapter { Id = chapter_id, },
                Content = new Content
                {
                    Type = ContentType.Html,
                    Text = html,
                },
            };
            mcont.Create();

            return Json(mcont.Pair);
        }

        [HttpPost]
        [Route("content/create/image")]
        [AuthorizeFilter(Scope = Scope.Write)]
        public IActionResult CreateImageContent(int chapter_id, short source, string extension, IList<IFormFile> data, string data_uri)
        {
            if ((MediaSource)source == MediaSource.Self)
            {
                // get data from form
                var base64 = data.First().ToBase64();

                // save data
                var mcont = new ContentModel
                {
                    AuthData = this.AuthData,
                    Chapter = new Chapter { Id = chapter_id, },
                    Content = new Content
                    {
                        Type = ContentType.Image,
                    },
                    Media = new Media
                    {
                        Source = MediaSource.Self,
                        Extension = MediaModel.StringToExtension(extension),
                        MediaData = base64,
                    },
                };
                mcont.Create();

                return Json(mcont.Pair);
            }
            else if ((MediaSource)source == MediaSource.Outside)
            {
                var mcont = new ContentModel
                {
                    AuthData = this.AuthData,
                    Chapter = new Chapter { Id = chapter_id, },
                    Content = new Content
                    {
                        Type = ContentType.Image,
                    },
                    Media = new Media
                    {
                        Source = MediaSource.Outside,
                        FilePath = data_uri,
                    },
                };
                mcont.Create();

                return Json(mcont.Pair);
            }

            throw new GunuccoException(new ApiMessage
            {
                StatusCode = 400,
                Message = "Invalid source value.",
            });
        }

        [HttpGet]
        [Route("content/{id}")]
        [AuthorizeFilter(IsCheckAuthorizable = false, Scope = Scope.Read)]
        public IActionResult GetContent(int id)
        {
            var mcont = new ContentModel
            {
                AuthData = this.AuthData,
                Content = new Content
                {
                    Id = id,
                },
            };
            mcont.LoadWithPermissionCheck();

            return Json(mcont.Pair);
        }

        [HttpPut]
        [Route("content/update")]
        [AuthorizeFilter(Scope = Scope.Write)]
        public IActionResult UpdateContent(string content)
        {
            var cont = this.LoadJson<Content>(content);

            var mcont = new ContentModel
            {
                AuthData = this.AuthData,
                Content = cont,
            };
            var mes = mcont.Save();

            return Json(mes);
        }

        [HttpDelete]
        [Route("content/delete")]
        [AuthorizeFilter(Scope = Scope.Write)]
        public IActionResult DeleteContent(int id)
        {
            var mcont = new ContentModel
            {
                AuthData = this.AuthData,
                Content = new Content
                {
                    Id = id,
                },
            };
            var mes = mcont.Delete();

            return Json(mes);
        }

        [HttpGet]
        [Route("download/media/{path}")]
        [AuthorizeFilter(IsCheckAuthorizable = false, Scope = Scope.Read)]
        public IActionResult DownloadMedia(string path)
        {
            var pair = ContentModel.GetPairFromMediaPath(this.AuthData, path);
            var mmedia = new MediaModel
            {
                AuthData = this.AuthData,
                Content = pair.Content,
                Media = pair.Media,
            };
            //mmedia.LoadMediaFromFile();
            
            return File(pair.Media.MediaDataRow, "image/" + pair.Media.Extension.ToString().ToLower());
        }

        [HttpGet]
        [Route("download/media/{path}/token")]
        public IActionResult DownloadMediaWithToken(string path, string token)
        {
            this.AuthData = Authentication.Authorize(token);
            if (!this.AuthData.AuthToken.Scope.HasFlag(Scope.Read))
            {
                throw new GunuccoException(new ApiMessage
                {
                    StatusCode = 403,
                    Message = "No scope to load this media.",
                });
            }

            return this.DownloadMedia(path);
        }

        #endregion

        #region BookPermission
        #endregion

        #region Timeline

        [HttpGet]
        [Route("timeline/local")]
        [AuthorizeFilter(IsCheckAuthorizable = false, Scope = Scope.Read)]
        public IActionResult GetLocalTimeline(int? num, int? min_id, int? max_id)
        {
            var mtl = new TimelineModel
            {
                AuthData = this.AuthData,
            };
            var tl = mtl.GetLocalItems(num ?? 20, min_id ?? 0, max_id ?? int.MaxValue);

            return Json(tl);
        }

        #endregion

        private T LoadJson<T>(string json)
        {
            try
            {
                return JsonConvert.DeserializeObject<T>(json);
            }
            catch
            {
                throw new GunuccoException(new ApiMessage
                {
                    StatusCode = 400,
                    Message = "Invalid json string.",
                });
            }
        }
    }
}
