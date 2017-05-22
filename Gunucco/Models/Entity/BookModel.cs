using Gunucco.Common;
using Gunucco.Entities;
using Gunucco.Models.Database;
using Gunucco.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gunucco.Models.Entity
{
    public class BookModel
    {
        public AuthenticationData AuthData { get; set; }

        public Book Book { get; set; }

        public void Create()
        {
            if (string.IsNullOrEmpty(this.Book.Name))
            {
                throw new GunuccoException(new ApiMessage
                {
                    StatusCode = 400,
                    Message = "No book name set",
                });
            }

            // setting object data
            this.Book.Created = DateTime.Now;
            var permission = new BookPermission
            {
                UserId = this.AuthData.User.Id,
                TargetType = TargetType.Book,
            };

            using (var db = new MainContext())
            {
                // create new book and get id
                db.Book.Add(this.Book);
                db.SaveChanges();

                // create new permission arrow the user read and write book
                permission.TargetId = this.Book.Id;
                db.BookPermission.Add(permission);
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
            var book = db.Book.Find(this.Book.Id);
            if (book == null)
            {
                throw new GunuccoException(new ApiMessage
                {
                    StatusCode = 404,
                    Message = "No such book id found.",
                });
            }

            this.Book = book;
        }

        public IQueryable<Book> GetUserBooks(int userId)
        {
            using (var db = new MainContext())
            {
                return GetUserBooks(db, userId);
            }
        }

        public static IQueryable<Book> GetUserBooks(MainContext db, int userId)
        {
            // get books
            var permissions = db.BookPermission.Where(p => p.UserId == userId).Where(p => p.TargetTypeValue == (short)TargetType.Book);
            var books = db.Book.Where(b => permissions.Any(p => p.TargetId == b.Id && p.TargetTypeValue == (short)TargetType.Book));
            return books;
        }

        public ApiMessage Delete()
        {
            using (var db = new MainContext())
            {
                return this.Delete(db);
            }
        }

        public ApiMessage Delete(MainContext db)
        {
            // get book detail data
            this.Load(db);

            // check permissions
            var permissions = this.GetPermissions(db);
            this.CheckPermission(permissions);

            // remove data
            db.Book.Attach(this.Book);
            db.Book.Remove(this.Book);
            db.BookPermission.RemoveRange(permissions);

            db.SaveChanges();

            return new ApiMessage
            {
                StatusCode = 200,
                Message = "Delete book succeed.",
            };
        }

        public IQueryable<BookPermission> GetPermissions(MainContext db)
        {
            return db.BookPermission.Where(p => p.UserId == this.AuthData.User.Id)
                                    .Where(p => p.TargetId == this.Book.Id &&
                                                p.TargetTypeValue == (short)TargetType.Book);
        }

        public void CheckPermission(MainContext db)
        {
            this.CheckPermission(this.GetPermissions(db));
        }

        public void CheckPermission(IQueryable<BookPermission> permissions)
        {
            if (!permissions.Any())
            {
                throw new GunuccoException(new ApiMessage
                {
                    StatusCode = 403,
                    Message = "No permission to write or delete books.",
                });
            }
        }
    }
}
