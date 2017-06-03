using Gunucco.Common;
using Gunucco.Entities;
using Gunucco.Models.Database;
using Gunucco.Models.Entities;
using Gunucco.Models.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gunucco.Models.Entity
{
    public class SessionModel
    {
        public AuthorizationData AuthData { get; set; }

        public IEnumerable<UserSessionData> GetData()
        {
            using (var db = new MainContext())
            {
                var sessions = db.UserSession.Where(s => s.UserId == this.AuthData.User.Id);
                return sessions.Select(s => new UserSessionData
                {
                    Expire = s.ExpireDateTime,
                    IdHash = CryptUtil.ToMD5(s.Id),
                    Scope = s.Scope,
                }).ToArray();
            }
        }

        public ApiMessage CancelAuthorization(string hashId)
        {
            using (var db = new MainContext())
            {
                var session = db.UserSession.Where(s => s.UserId == this.AuthData.User.Id)
                    .SingleOrDefault(s => CryptUtil.ToMD5(s.Id) == hashId);
                if (session == null)
                {
                    throw new GunuccoException(new ApiMessage
                    {
                        StatusCode = 404,
                        Message = "No such user session found.",
                    });
                }

                db.UserSession.Remove(session);
                db.SaveChanges();
            }

            return new ApiMessage
            {
                StatusCode = 200,
                Message = "Delete a session succeed.",
            };
        }
    }
}
