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
        [Route("book/get/user/{id}")]
        public IActionResult GetUserBooks(int userId)
        {
            var mbook = new BookModel
            {
                AuthData = this.AuthData,
            };
            var books = mbook.GetUserBooks(userId);

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
        #endregion

        #region Content and Media
        #endregion

        #region BookPermission
        #endregion
    }
}
