using Gunucco.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gunucco.Controllers
{
    [AccessTokenSessionActionFilter]
    public abstract class GunuccoWebControllerBase : Controller, IAccessTokenSession
    {
        public string AccessTokenSession
        {
            get
            {
                var sessions = this.HttpContext.Session;
                if (sessions.Keys.Contains("AuthToken"))
                {
                    return sessions.GetString("AuthToken");
                }
                return null;
            }
            set
            {
                this.HttpContext.Session.SetString("AuthToken", value);
            }
        }
    }
}
