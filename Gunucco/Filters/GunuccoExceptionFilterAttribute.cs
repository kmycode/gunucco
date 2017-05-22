using Gunucco.Common;
using Gunucco.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gunucco.Filters
{
    public class GunuccoExceptionFilterAttribute : Attribute, IExceptionFilter
    {
        private static Logger log = LogManager.GetCurrentClassLogger();

        public void OnException(ExceptionContext context)
        {
            // 400: Bad Request
            // 401: Unautorized
            // 403: Forbidden
            // 405: Method Not Allowed
            // 415: Unsupported Media Type
            // 501: Not Implemented
            // 503: Service Unavailable
            if (context.Exception is GunuccoException e)
            {
                context.HttpContext.Response.StatusCode = e.Error.StatusCode;
                if (e.Error is BearerApiMessage bm)
                {
                    context.HttpContext.Response.Headers.Add("WWW-Authenticate", bm.WWWAuthenticateMessage);
                }
                context.Result = new JsonResult(e.Error);

                log.Debug("[Exception] Handled error on route: " + context.HttpContext.Request.Path +
                          " (METHOD " + context.HttpContext.Request.Method + ") (HTTP " + e.Error.StatusCode + ") " + e.Error.Message);
            }
            else
            {
                context.HttpContext.Response.StatusCode = 503;
                context.Result = new JsonResult(new ApiMessage
                {
                    StatusCode = 503,
                    Message = "Service unavailable.",
                });

                log.Error(context.Exception, "[Exception] Un-handled error on route: (" + context.HttpContext.Request.Method + ") " + context.HttpContext.Request.Path);
            }
        }
    }
}
