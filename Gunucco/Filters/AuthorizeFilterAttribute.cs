using Gunucco.Common;
using Gunucco.Controllers;
using Gunucco.Entities;
using Gunucco.Models;
using Gunucco.Models.Entities;
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
        public bool IsCheckAuthorizable { get; set; } = true;

        public Scope Scope { get; set; } = Scope.None;

        public AuthorizeFilterAttribute()
        {
            this.Order = 1;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);
            var authStr = context.HttpContext.Request.Headers["Authorization"];
            AuthorizationData data = null;

            if (!this.IsCheckAuthorizable && string.IsNullOrEmpty(authStr))
            {
                // no need to authorize
                data = new AuthorizationData
                {
                    AuthToken = new AuthorizationToken(),
                    Session = new UserSession(),
                    User = new User(),
                };
            }
            else
            {
                try
                {
                    data = Authentication.Authorize(authStr);
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

                // scope check
                if (data?.Session == null || !data.Session.Scope.HasFlag(this.Scope))
                {
                    if (this.IsCheckAuthorizable)
                    {
                        context.HttpContext.Response.StatusCode = 403;
                        context.Result = new JsonResult(new ApiMessage
                        {
                            StatusCode = 403,
                            Message = "You don't have scope to action.",
                        });
                    }
                    else
                    {
                        // make situation no auth
                        data = new AuthorizationData
                        {
                            AuthToken = new AuthorizationToken(),
                            Session = new UserSession(),
                            User = new User(),
                        };
                    }
                }
            }

            ((Api1Controller)context.Controller).AuthData = data;
        }
    }
}
