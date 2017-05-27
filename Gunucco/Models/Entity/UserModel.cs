using Gunucco.Common;
using Gunucco.Entities;
using Gunucco.Models.Database;
using Gunucco.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gunucco.Models.Entity
{
    public class UserModel
    {
        public AuthorizationData AuthData { get; set; }

        public void Create(string id, string password)
        {
            this.CheckPasswordSafety(password);

            var token = new AuthorizationToken
            {
                UserTextId = id,
            };

            if (string.IsNullOrEmpty(id))
            {
                throw new GunuccoException(new ApiMessage
                {
                    StatusCode = 400,
                    Message = "Create user failed. Id is too short.",
                });
            }
            if (id.Length > 30)
            {
                throw new GunuccoException(new ApiMessage
                {
                    StatusCode = 400,
                    Message = "Create user failed. Id is too long.",
                });
            }

            var user = new User
            {
                TextId = id,
                Name = id,
            };
            user.SetPassword(password);

            using (var db = new MainContext())
            {
                if (db.User.Any(u => u.TextId == id))
                {
                    throw new GunuccoException(new ApiMessage
                    {
                        StatusCode = 400,
                        Message = "Create user failed. Existing user id.",
                    });
                }

                db.User.Add(user);
                db.SaveChanges();
            }

            this.AuthData = new AuthorizationData
            {
                User = user,
                AuthToken = token,
            };
        }

        public ApiMessage Delete()
        {
            return this.Delete(this.AuthData.User.Id);
        }

        public ApiMessage Delete(int id)
        {
            using (var db = new MainContext())
            {
                var user = db.User.Find(id);
                if (user == null)
                {
                    throw new GunuccoException(new ApiMessage
                    {
                        StatusCode = 404,
                        Message = "Delete user failed. No such user id.",
                    });
                }

                // remove user's books (user own only)
                var books = BookModel.GetUserBooks(db, user.Id);
                foreach (var book in books)
                {
                    var mbook = new BookModel
                    {
                        AuthData = this.AuthData,
                        Book = book,
                    };
                    var permissions = mbook.GetPermissions(db);

                    // delete book if the user own book
                    if (permissions.Count() == 1)
                    {
                        mbook.Delete(db);
                    }
                    else
                    {
                        // delete book permission only
                        db.BookPermission.Remove(permissions.Single(p => p.UserId == user.Id));
                    }
                }

                // remove user
                db.User.Remove(user);

                db.SaveChanges();
            }

            return new ApiMessage
            {
                StatusCode = 200,
                Message = "Delete user succeed.",
            };
        }

        private void CheckPasswordSafety(string password)
        {
            if (string.IsNullOrEmpty(password) || password.Length < 6)
            {
                throw new GunuccoException(new ApiMessage
                {
                    StatusCode = 400,
                    Message = "Create user failed. Password is too short.",
                });
            }
        }
    }
}
