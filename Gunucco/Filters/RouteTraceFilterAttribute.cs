using Microsoft.AspNetCore.Mvc.Filters;
using NLog;
using NLog.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gunucco.Filters
{
    public class RouteTraceFilterAttribute : ActionFilterAttribute
    {
        private static Logger log = LogManager.GetCurrentClassLogger();

        public override void OnResultExecuted(ResultExecutedContext context)
        {
            base.OnResultExecuted(context);

            log.Trace("  route: " + context.HttpContext.Request.Path +
                          " (METHOD " + context.HttpContext.Request.Method + ") (HTTP " + context.HttpContext.Response.StatusCode + ") ");
        }
    }
}
