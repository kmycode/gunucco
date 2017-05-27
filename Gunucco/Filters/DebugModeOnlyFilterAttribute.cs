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
    public class DebugModeOnlyFilterAttribute : ActionFilterAttribute
    {
        private static Logger log = LogManager.GetCurrentClassLogger();

        public DebugModeOnlyFilterAttribute()
        {
            this.Order = 2;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);

            if (!Config.IsDebugMode)
            {
                context.HttpContext.Response.StatusCode = 403;
                context.Result = new JsonResult(new ApiMessage
                {
                    StatusCode = 403,
                    Message = "This action is arrowed only debug mode.",
                });

                log.Debug("[Exception] action only debug mode called: " + context.HttpContext.Request.Path +
                          " (METHOD " + context.HttpContext.Request.Method + ")");
            }
        }
    }
}
