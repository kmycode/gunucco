using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gunucco.Filters
{
    public class AccessTokenSessionActionFilter : ActionFilterAttribute
    {
        public override void OnResultExecuting(ResultExecutingContext context)
        {
            base.OnResultExecuting(context);

            var controller = (Controller)context.Controller;
            if (controller is IAccessTokenSession accessTokenSession &&
                !string.IsNullOrEmpty(accessTokenSession.AccessTokenSession))
            {
                controller.ViewBag.AccessToken = accessTokenSession.AccessTokenSession;
                controller.ViewBag.IsAuthenticated = true;
            }
            else
            {
                controller.ViewBag.AccessToken = null;
                controller.ViewBag.IsAuthenticated = false;
            }
        }
    }

    public interface IAccessTokenSession
    {
        string AccessTokenSession { get; }
    }
}
