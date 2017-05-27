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

        [Route("signup")]
        public IActionResult SignUp()
        {
            return View();
        }

        public IActionResult SignIn()
        {
            return View();
        }
    }
}
