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
        [Route("book/get/{id}")]
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
        [Route("book/get/{id}/chapters")]
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
        [Route("book/get/{id}/chapters/root")]
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

        [HttpGet]
        [Route("book/get/user/{id}")]
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
        public IActionResult CreateChapter(string name, int bookId)
        {
            var mchap = new ChapterModel
            {
                AuthData = this.AuthData,
                Book = new Book
                {
                    Id = bookId,
                },
                Chapter = new Chapter
                {
                    Name = name,
                    BookId = bookId,
                    PublicRange = PublishRange.Private,
                },
            };
            mchap.Create();

            return Json(mchap.Chapter);
        }

        [HttpGet]
        [Route("chapter/get/{id}")]
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
        [Route("chapter/get/{id}/children")]
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

        [HttpPut]
        [Route("chapter/update")]
        [AuthorizeFilter]
        public IActionResult UpdateChapter(string chapter)
        {
            Chapter chap = null;
            try
            {
                chap = JsonConvert.DeserializeObject<Chapter>(chapter);
            }
            catch
            {
                throw new GunuccoException(new ApiMessage
                {
                    StatusCode = 400,
                    Message = "Invalid chapter json string.",
                });
            }

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
        #endregion

        #region BookPermission
        #endregion
    }
}
