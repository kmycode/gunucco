using Gunucco.Common;
using Gunucco.Entities;
using Gunucco.Models.Database;
using Gunucco.Models.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gunucco.Models
{
    public static class Authentication
    {
        private static Logger log = LogManager.GetCurrentClassLogger();

        public static AuthorizationData Authorize(string authString)
        {
            AuthorizationToken token = null;

            if (string.IsNullOrWhiteSpace(authString))
            {
                throw new GunuccoException(new BearerApiMessage
                {
                    StatusCode = 401,
                    Message = "Login failed. Invalid authorization header.",
                    WWWAuthenticateMessage = "Bearer realm=\"gunucco_realm\"",
                });
            }

            // get user data from authorization header
            authString = authString.ToString().Replace("Bearer ", "");
            token = ParseToken(authString);

            // check session been availabled
            User user;
            UserSession session;
            using (var db = new MainContext())
            {
                user = CheckLoginable(db, token);
                session = db.UserSession.Find(token.SessionId);
            }
            if (session == null)
            {
                throw new GunuccoException(new BearerApiMessage
                {
                    StatusCode = 401,
                    Message = "Login failed. No session found or session has already expired.",
                    WWWAuthenticateMessage = "Bearer error=\"invalid_token\"",
                });
            }
            if (session.ExpireDateTime < DateTime.Now)
            {
                throw new GunuccoException(new BearerApiMessage
                {
                    StatusCode = 401,
                    Message = "Login failed. Session has already expired.",
                    WWWAuthenticateMessage = "Bearer error=\"invalid_token\"",
                });
            }
            token.Scope = session.Scope;

            return new AuthorizationData
            {
                User = user,
                Session = session,
                AuthToken = token,
            };
        }

        public static AuthorizationData Authorize(string id, string password, Scope scope = Scope.LegacyFull)
        {
            var token = new AuthorizationToken
            {
                UserTextId = id,
            };

            if (string.IsNullOrEmpty(password))
            {
                throw new GunuccoException(new BearerApiMessage
                {
                    StatusCode = 400,
                    Message = "Login failed. Password is empty.",
                    WWWAuthenticateMessage = "Bearer error=\"invalid_request\"",
                });
            }

            User user;
            UserSession session;
            using (var db = new MainContext())
            {
                // create new session
                user = CheckLoginable(db, token, password);
                session = UserSession.Create(user, scope);

                db.UserSession.Add(session);
                db.SaveChanges();
            }
            token.SessionId = session.Id;
            token.AccessToken = GetBearerToken(session);

            // cleaning expired sessions per 1 hour
            CleanExpiredSessions();

            return new AuthorizationData
            {
                User = user,
                Session = session,
                AuthToken = token,
            };
        }

        private static User CheckLoginable(MainContext db, AuthorizationToken token, string password = null)
        {
            User user;
            
            if (token.UserId != 0)
            {
                user = db.User.Find(token.UserId);
            }
            else
            {
                user = db.User.SingleOrDefault(u => u.TextId == token.UserTextId);
            }

            if (user == null)
            {
                throw new GunuccoException(new BearerApiMessage
                {
                    StatusCode = 400,
                    Message = "Login failed. No such user.",
                    WWWAuthenticateMessage = "Bearer error=\"invalid_request\"",
                });
            }
            else if (user.TextId != token.UserTextId || (password != null && !user.IsMatchPassword(password)))
            {
                throw new GunuccoException(new BearerApiMessage
                {
                    StatusCode = 400,
                    Message = "Login failed. Invalid id or password.",
                    WWWAuthenticateMessage = "Bearer error=\"invalid_request\"",
                });
            }

            token.UserId = user.Id;

            return user;
        }

        private static string GetBearerToken(UserSession session)
        {
            return Convert.ToBase64String(Encoding.ASCII.GetBytes(session.UserId + ":" + session.UserTextId + ":" + session.Id));
        }

        private static AuthorizationToken ParseToken(string bearerToken)
        {
            string str = null;
            try
            {
                str = Encoding.ASCII.GetString(Convert.FromBase64String(bearerToken));
            }
            catch
            {
                throw new GunuccoException(new BearerApiMessage
                {
                    StatusCode = 401,
                    Message = "Login failed. Invalid access token.",
                    WWWAuthenticateMessage = "Bearer error=\"invalid_token\"",
                });
            }

            var cred = str.Split(':');
            if (cred.Count() < 3)
            {
                throw new GunuccoException(new BearerApiMessage
                {
                    StatusCode = 401,
                    Message = "Login failed. Invalid access token.",
                    WWWAuthenticateMessage = "Bearer error=\"invalid_token\"",
                });
            }

            var id = int.Parse(cred[0]);

            return new AuthorizationToken
            {
                UserId = id,
                UserTextId = cred[1],
                SessionId = cred[2],
            };
        }

        private static DateTime nextCleanExpiredSessionTime = DateTime.MinValue;
        private static readonly object cleanExpiredSessionsLocker = new object();
        private static async Task CleanExpiredSessions()
        {
            var now = DateTime.Now;
            lock (cleanExpiredSessionsLocker)
            {
                if (nextCleanExpiredSessionTime > now) return;
            }

            nextCleanExpiredSessionTime = now.AddHours(1);

            // 6 hours after expired to output 'token already expired' error when user logined old token
            var removeDateTime = now.AddHours(-6);

            log.Info("[Start] Cleaning expired sessions");
            log.Info("    delete sessions until " + removeDateTime.ToString("yyyy/MM/dd HH:mm:ss") + ".");

            int deleteCount = 0;
            try
            {
                using (var db = new MainContext())
                {
                    var expireds = db.UserSession.Where(e => e.ExpireDateTime < removeDateTime);
                    foreach (var ee in expireds)
                    {
                        db.UserSession.Remove(ee);
                        deleteCount++;
                    }
                    await db.SaveChangesAsync();
                }
            }
            catch (Exception e)
            {
                log.Error(e, "    exception occured while using database.");
            }

            log.Info("    deleted " + (deleteCount == 0 ? "no" : deleteCount.ToString()) + " session" + (deleteCount <= 1 ? "." : "s."));
            log.Info("[End] Cleaning expired sessions");
        }
    }
}
