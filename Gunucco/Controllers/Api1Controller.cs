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

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Gunucco.Controllers
{
    [Route("/api/v1")]
    [ArrowCrossDomain]
    [GunuccoExceptionFilter]
    [RouteTraceFilter]
    public class Api1Controller : Controller
    {
        public AuthenticationData AuthData { get; set; }

        public IActionResult Index()
        {
            throw new GunuccoException(new ApiMessage
            {
                StatusCode = 501,
                Message = "Not implemented action.",
            });
        }

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

        [HttpDelete]
        [Route("user/delete")]
        [AuthorizeFilter]
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
            var data = Authentication.Authenticate(id, password);

            return Json(data.AuthToken);
        }

        [HttpGet]
        [Route("user/{id}/books")]
        [AuthorizeFilter(IsCheckAuthorizable = false)]
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
        [AuthorizeFilter]
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
        [AuthorizeFilter(IsCheckAuthorizable = false)]
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
        [AuthorizeFilter(IsCheckAuthorizable = false)]
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
        [AuthorizeFilter(IsCheckAuthorizable = false)]
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

        [HttpDelete]
        [Route("book/delete")]
        [AuthorizeFilter]
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
        [AuthorizeFilter]
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
        [AuthorizeFilter(IsCheckAuthorizable = false)]
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
        [AuthorizeFilter(IsCheckAuthorizable = false)]
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
        [AuthorizeFilter(IsCheckAuthorizable = false)]
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
        [AuthorizeFilter]
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
        [AuthorizeFilter]
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
        [AuthorizeFilter]
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
        [Route("content/create/image")]
        [AuthorizeFilter]
        public IActionResult CreateImageContent(int chapter_id, short source, string extension, IList<IFormFile> data, string data_uri)
        {
            if ((MediaSource)source == MediaSource.Self)
            {
                // get data from form
                var stream = data.First().OpenReadStream();
                var buf = new byte[stream.Length];
                stream.Seek(0, SeekOrigin.Begin);
                stream.Read(buf, 0, (int)stream.Length);
                var base64 = Convert.ToBase64String(buf);

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
        [AuthorizeFilter(IsCheckAuthorizable = false)]
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
        [AuthorizeFilter]
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
        [AuthorizeFilter]
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
        [AuthorizeFilter(IsCheckAuthorizable = false)]
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

        #endregion

        #region BookPermission
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
