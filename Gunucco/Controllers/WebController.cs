using Gunucco.Common;
using Gunucco.Entities;
using Gunucco.Models;
using Gunucco.Models.Entities;
using Gunucco.Models.Entity;
using Gunucco.ViewModels;
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
            return View();
        }

        [HttpGet]
        [Route("signup")]
        public IActionResult SignUp()
        {
            return View();
        }

        [HttpPost]
        [Route("signup")]
        public IActionResult SignUp_Post(string text_id, string password)
        {
            if (string.IsNullOrEmpty(text_id) || string.IsNullOrEmpty(password))
            {
                return this.ShowMessage("Text id or password isn't set.");
            }

            try
            {
                var muser = new UserModel();
                muser.Create(text_id, password);
            }
            catch (GunuccoException ex)
            {
                this.HttpContext.Response.StatusCode = ex.Error.StatusCode;
                return this.ShowMessage(ex.Error.Message);
            }

            return this.ShowMessage("Sign in succeed. Welcome to Gunucco!", false);
        }

        public IActionResult SignIn()
        {
            return View();
        }

        // [HttpPost]
        [Route("mypage")]
        public IActionResult MyPage(string auth_token)
        {
            // TODO: stub
            var authData = Authentication.Authorize("kmys", "takaki", Scope.WebClient);

            var mbook = new BookModel
            {
                AuthData = authData,
            };
            var books = mbook.GetUserBooks(authData.User.Id);

            var vm = new MyPageTopViewModel
            {
                AuthData = authData,
                Books = books,
            };

            return View(vm);
        }

        [HttpPost]
        [Route("mypage/book/new")]
        public IActionResult CreateNewBook(string auth_token, string book_name)
        {
            // TODO: stub
            var authData = Authentication.Authorize("kmys", "takaki", Scope.WebClient);

            var mbook = new BookModel
            {
                AuthData = authData,
                Book = new Book
                {
                    Name = book_name,
                },
            };

            MessageViewModel mes = new MessageViewModel();
            try
            {
                mbook.Create();

                mes.HasMessage = true;
                mes.IsError = false;
                mes.Message = "Creating new book succeed.";
            }
            catch (GunuccoException ex)
            {
                mes.HasMessage = true;
                mes.IsError = true;
                mes.Message = ex.Error.Message;
            }
            var books = mbook.GetUserBooks(authData.User.Id);

            var vm = new MyPageTopViewModel
            {
                AuthData = authData,
                Books = books,
                Message = mes,
            };

            return View("MyPage", vm);
        }

        private IActionResult ShowMessage(string mes, bool isError = true)
        {
            ViewBag.Message = mes;
            ViewBag.IsError = isError;
            return View("Message");
        }
    }
}
