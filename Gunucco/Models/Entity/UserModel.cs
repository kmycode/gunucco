using Gunucco.Common;
using Gunucco.Entities;
using Gunucco.Models.Database;
using Gunucco.Models.Entities;
using Gunucco.Models.Utils;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Gunucco.Models.Entity
{
    public class UserModel
    {
        public AuthorizationData AuthData { get; set; }

        public User User { get; set; }

        private bool isLoaded = false;

        public void Create(string id, string password, string email = "unset", bool isEmailValidated = true)
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
            if (!Regex.IsMatch(id, "^[0-9a-zA-Z]+$"))
            {
                throw new GunuccoException(new ApiMessage
                {
                    StatusCode = 400,
                    Message = "Create user failed. Id must be only numbers or alphabets.",
                });
            }

            var user = new User
            {
                TextId = id,
                Name = id,
                IsEmailValidated = isEmailValidated,
            };
            user.SetPassword(password);
            user.SetEmail(email);

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

                if (Config.IsEmailValidationNeed)
                {
                    if (db.User.Any(u => u.EmailHash == user.EmailHash))
                    {
                        throw new GunuccoException(new ApiMessage
                        {
                            StatusCode = 400,
                            Message = "Create user failed. Existing email.",
                        });
                    }
                }

                db.User.Add(user);
                db.SaveChanges();
            }

            this.AuthData = new AuthorizationData
            {
                User = user,
                AuthToken = token,
            };
            this.User = user;
        }

        public void SendValidationEmail(string email)
        {
            var key = CryptUtil.CreateKey(64);

            using (var db = new MainContext())
            {
                db.UserEmailValidation.Add(new UserEmailValidation
                {
                    UserId = this.User.Id,
                    ValidateKey = key,
                });
                db.SaveChanges();
            }

            try
            {
                var mail = MailSender.Create();
                mail.Subject = "Activate your Gunucco account";
                mail.To = email;
                mail.Text = $@"Dear {this.User.Name},

Thank you for sign up {Config.ServerPath}, a Gunucco server.
Click following link, you can activate your account.

{Config.ServerPath}/web/signup/activate?user_id={this.User.Id}&key={Uri.EscapeDataString(key)}

== NOTICE ==
If you don't know about this email, please ignore.

Thanks.
";
                mail.Send();
            }
            catch
            {
                this.Delete();

                throw new GunuccoException(new ApiMessage
                {
                    StatusCode = 500,
                    Message = "Email cannot send because can't connect SMTP server. Please contact server administrator.",
                });
            }
        }

        public void CheckActivateKeyValidation(string key)
        {
            using (var db = new MainContext())
            {
                var validation = db.UserEmailValidation.FirstOrDefault(v => v.ValidateKey == key);
                if (validation == null)
                {
                    throw new GunuccoException(new ApiMessage
                    {
                        StatusCode = 404,
                        Message = "No such validation key found.",
                    });
                }

                db.UserEmailValidation.Remove(validation);

                this.Load(db);
                db.User.Attach(this.User);
                this.User.IsEmailValidated = true;
                db.SaveChanges();
            }
        }

        public void Load()
        {
            using (var db = new MainContext())
            {
                this.Load(db);
            }
        }

        public void Load(MainContext db)
        {
            if (this.isLoaded) return;
            this.isLoaded = true;

            var user = db.User.Find(this.User.Id);
            if (user == null)
            {
                throw new GunuccoException(new ApiMessage
                {
                    StatusCode = 404,
                    Message = "No such user id found.",
                });
            }

            this.User = user;
        }

        public void LoadWithTextId()
        {
            using (var db = new MainContext())
            {
                this.LoadWithTextId(db);
            }
        }

        public void LoadWithTextId(MainContext db)
        {
            if (this.isLoaded) return;
            this.isLoaded = true;

            var user = db.User.SingleOrDefault(u => u.TextId == this.User.TextId);
            if (user == null)
            {
                throw new GunuccoException(new ApiMessage
                {
                    StatusCode = 404,
                    Message = "No such user id found.",
                });
            }

            this.User = user;
        }

        public static UserModel FromIdOrAnonymous(int? id)
        {
            var muser = new UserModel
            {
                User = new User(),
            };

            try
            {
                muser.User.Id = id.Value;
                muser.Load();
            }
            catch
            {
                muser.User.Name = "Anonymous";
                muser.User.IsAnonymous = true;
            }

            return muser;
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
