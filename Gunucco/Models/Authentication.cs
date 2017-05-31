using Gunucco.Common;
using Gunucco.Entities;
using Gunucco.Models.Database;
using Gunucco.Models.Entities;
using Gunucco.Models.Utils;
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
            token.AccessToken = authString;

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
            DBCleanerUtil.CleanUserSession();

            return new AuthorizationData
            {
                User = user,
                Session = session,
                AuthToken = token,
            };
        }

        #region oauth

        public static OauthCode CreateOauthCode(Scope scope)
        {
            if (scope.HasFlag(Scope.WebClient))
            {
                throw new GunuccoException(new ApiMessage
                {
                    StatusCode = 403,
                    Message = "Invalid scope for creating oauth code.",
                });
            }

            var code = new OauthCode
            {
                Code = CryptUtil.CreateKey(64),
                ExpireDateTime = DateTime.Now.AddHours(2),
                Scope = scope,
            };

            using (var db = new MainContext())
            {
                db.OauthCode.Add(code);
                db.SaveChanges();
            }

            DBCleanerUtil.CleanOauthCode();

            code.OauthUri = Config.ServerPath + "/web/oauth?code=" + Uri.EscapeDataString(code.Code);

            return code;
        }

        public static Scope GetOauthCodeScopeForOauthRequest(string code)
        {
            using (var db = new MainContext())
            {
                // check oauth data
                var codeData = db.OauthCode.SingleOrDefault(o => o.Code == code);
                if (codeData == null)
                {
                    throw new GunuccoException(new ApiMessage
                    {
                        StatusCode = 400,
                        Message = "No such oauth code found.",
                    });
                }
                if (!string.IsNullOrWhiteSpace(codeData.SessionId))
                {
                    throw new GunuccoException(new ApiMessage
                    {
                        StatusCode = 400,
                        Message = "This oauth code is already authorized.",
                    });
                }

                return codeData.Scope;
            }
        }

        public static void AuthorizeWithOauth(string code, AuthorizationData webClientAuthData)
        {
            if (!webClientAuthData.Session.Scope.HasFlag(Scope.WebClient))
            {
                throw new GunuccoException(new ApiMessage
                {
                    StatusCode = 403,
                    Message = "Login failed. Invalid web-client scope for oauth.",
                });
            }

            using (var db = new MainContext())
            {
                // check oauth data
                var codeData = db.OauthCode.SingleOrDefault(o => o.Code == code);
                if (codeData == null)
                {
                    throw new GunuccoException(new ApiMessage
                    {
                        StatusCode = 400,
                        Message = "No such oauth code found.",
                    });
                }
                if (!string.IsNullOrWhiteSpace(codeData.SessionId))
                {
                    throw new GunuccoException(new ApiMessage
                    {
                        StatusCode = 400,
                        Message = "This oauth code is already authorized.",
                    });
                }

                // create new session
                var session = UserSession.Create(new User
                {
                    Id = webClientAuthData.User.Id,
                    TextId = webClientAuthData.User.TextId,
                }, codeData.Scope);
                db.UserSession.Add(session);

                // save access token
                codeData.SessionId = session.Id;
                codeData.UserId = webClientAuthData.User.Id;
                codeData.UserTextId = webClientAuthData.User.TextId;

                db.SaveChanges();
            }
        }

        public static AuthorizationToken GetTokenWithAuthCode(string code)
        {
            AuthorizationToken token = new AuthorizationToken();
            UserSession session = null;

            using (var db = new MainContext())
            {
                // check oauth data
                var codeData = db.OauthCode.SingleOrDefault(o => o.Code == code);
                if (codeData == null)
                {
                    throw new GunuccoException(new ApiMessage
                    {
                        StatusCode = 400,
                        Message = "No such oauth code found.",
                    });
                }
                if (string.IsNullOrWhiteSpace(codeData.SessionId))
                {
                    throw new GunuccoException(new ApiMessage
                    {
                        StatusCode = 400,
                        Message = "This oauth code is not authorized.",
                    });
                }

                // get session
                session = db.UserSession.Find(codeData.SessionId);
                if (session == null)
                {
                    throw new GunuccoException(new ApiMessage
                    {
                        StatusCode = 400,
                        Message = "No such session found.",
                    });
                }

                // get data for build access token
                token.UserId = codeData.UserId;
                token.UserTextId = codeData.UserTextId;
                token.Scope = codeData.Scope;

                // remove auth code
                db.OauthCode.Remove(codeData);

                db.SaveChanges();
            }

            token.AccessToken = GetBearerToken(session);

            return token;
        }

        #endregion

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
            if (user.TextId != token.UserTextId || (password != null && !user.IsMatchPassword(password)))
            {
                throw new GunuccoException(new BearerApiMessage
                {
                    StatusCode = 400,
                    Message = "Login failed. Invalid id or password.",
                    WWWAuthenticateMessage = "Bearer error=\"invalid_request\"",
                });
            }
            if ((Config.IsEmailValidationNeed && !Config.IsDebugMode) && !user.IsEmailValidated)
            {
                throw new GunuccoException(new ApiMessage
                {
                    StatusCode = 400,
                    Message = "Login failed. This user email isn't activated.",
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
    }
}
