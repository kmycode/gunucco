using Gunucco.Common;
using Gunucco.Controllers;
using Gunucco.Entities;
using Gunucco.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gunucco.Filters
{
    public class AuthorizeFilterAttribute : ActionFilterAttribute
    {
        public AuthorizeFilterAttribute()
        {
            this.Order = 1;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);

            try
            {
                var data = Authentication.Authenticate(context.HttpContext.Request);
                ((Api1Controller)context.Controller).AuthData = data;
            }
            catch (GunuccoException e)
            {
                context.HttpContext.Response.StatusCode = e.Error.StatusCode;
                context.Result = new JsonResult(e.Error);
            }
            catch
            {
                context.HttpContext.Response.StatusCode = 503;
                context.Result = new JsonResult(new ApiMessage
                {
                    StatusCode = 503,
                    Message = "Service unavailable.",
                });
            }
        }
    }
}
