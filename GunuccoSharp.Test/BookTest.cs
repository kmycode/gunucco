using Gunucco.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GunuccoSharp.Test
{
    [TestClass]
    public class BookTest
    {
        [TestCleanup]
        public async Task Cleanup()
        {
            await TestUtil.CleanupAsync();
        }

        [DataTestMethod]
        [DataRow("Test book")]
        [DataRow("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa")]
        public async Task CreateBook(string name)
        {
            var client = await TestUtil.GetUserClientAsync();

            // create book
            var book = await TestUtil.Books.CreateAsync(client, new Book
            {
                Name = name,
            });

            Assert.IsNotNull(book);
            Assert.AreNotEqual(book.Id, 0);
            Assert.IsTrue(book.Created > DateTime.Now.AddMinutes(-3));
            Assert.AreEqual(book.Name, name);
        }

        [DataTestMethod]
        [DataRow("")]
        [DataRow(null)]
        [DataRow("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa")]
        public async Task CreateBook_Failed_InvalidBookName(string bookName)
        {
            var client = await TestUtil.GetUserClientAsync();

            // create book
            var e = await Assert.ThrowsExceptionAsync<GunuccoErrorException>(async () =>
            {
                await TestUtil.Books.CreateAsync(client, new Book
                {
                    Name = bookName,
                });
            });

            Assert.IsNotNull(e.Error);
            Assert.AreEqual(e.Error.StatusCode, 400);
            Assert.IsTrue(e.Error.Message.Contains("too"));
        }

        [TestMethod]
        public async Task GetBook()
        {
            var client = await TestUtil.GetUserClientAsync();

            // create book
            var book = await TestUtil.Books.CreateAsync(client);

            // get book data
            var book_get = await client.Book.GetAsync(book.Id);

            Assert.IsNotNull(book_get);
            Assert.AreEqual(book_get.Id, book.Id);
            Assert.AreEqual(book_get.Name, book.Name);
        }

        [DataTestMethod]
        [DataRow(-1)]
        [DataRow(null)]         // last book id + 1 (test more than 0 value)
        public async Task GetBook_Failed_InvalidBookId(int? id)
        {
            var client = await TestUtil.GetUserClientAsync();

            // create dummy book
            var book = await TestUtil.Books.CreateAsync(client, 0);
            if (id == null)
            {
                id = book.Id + 1;
            }

            // get book data
            var e = await Assert.ThrowsExceptionAsync<GunuccoErrorException>(async () =>
            {
                await client.Book.GetAsync(id.Value);
            });

            Assert.AreEqual(e.StatusCode, 404);
            Assert.IsNotNull(e.Error);
            Assert.IsTrue(e.Error.Message.Contains("No such book id found"));
        }

        [DataTestMethod]
        [DataRow(1)]
        [DataRow(3)]
        [DataRow(0)]
        public async Task GetBookChapters(int chapterCount)
        {
            var client = await TestUtil.GetUserClientAsync();

            // create book
            var book = await TestUtil.Books.CreateAsync(client);

            // create root chapter
            var chap = await TestUtil.Chapters.CreateAsync(client, 0, book.Id);

            // create chapter
            for (int i = 0; i < chapterCount; i++)
            {
                var child = await TestUtil.Chapters.CreateAsync(client, 0, book.Id);
                child.ParentId = chap.Id;
                await client.Chapter.UpdateAsync(child);
            }

            // get children chapters
            var chapters = await client.Book.GetChaptersAsync(book.Id);

            Assert.IsNotNull(chapters);
            Assert.AreEqual(chapters.Count(), chapterCount + 1);    // children and parent
            Assert.IsTrue(chapters.All(c => c.BookId == book.Id));
        }

        [TestMethod]
        public async Task GetBookChapters_Failed_InvalidBookId()
        {
            var client = await TestUtil.GetUserClientAsync();

            // create book
            var book = await TestUtil.Books.CreateAsync(client);

            // get children chapters
            var ex = await Assert.ThrowsExceptionAsync<GunuccoErrorException>(async () =>
            {
                await client.Book.GetChaptersAsync(book.Id + 1);
            });

            TestUtil.CheckException(ex, 404, "found");
        }

        [DataTestMethod]
        [DataRow(1, 0)]
        [DataRow(3, 0)]
        [DataRow(1, 1)]
        [DataRow(3, 5)]
        [DataRow(0, 0)]
        public async Task GetBookRootChapters(int chapterCount, int chapterChildCount)
        {
            var client = await TestUtil.GetUserClientAsync();

            // create book
            var book = await TestUtil.Books.CreateAsync(client);
            
            // create chapter
            for (int i = 0; i < chapterCount; i++)
            {
                var chap = await TestUtil.Chapters.CreateAsync(client, 0, book.Id);

                // create grandson chapter
                if (i == 0)
                {
                    for (int j = 0; j < chapterChildCount; j++)
                    {
                        var child = await TestUtil.Chapters.CreateAsync(client, 1, book.Id);
                        child.ParentId = chap.Id;
                        await client.Chapter.UpdateAsync(child);
                    }
                }
            }

            // get children chapters
            var chapters = await client.Book.GetRootChaptersAsync(book.Id);

            Assert.IsNotNull(chapters);
            Assert.AreEqual(chapters.Count(), chapterCount);
            Assert.IsTrue(chapters.All(c => c.BookId == book.Id));
        }

        [TestMethod]
        public async Task GetUserBooks()
        {
            var client1 = await TestUtil.GetUserClientAsync(0);
            var client2 = await TestUtil.GetUserClientAsync(1);
            var client3 = await TestUtil.GetUserClientAsync(2);

            // create test books
            var book1 = await TestUtil.Books.CreateAsync(client1, 0);
            var book2 = await TestUtil.Books.CreateAsync(client1, 1);
            var book3 = await TestUtil.Books.CreateAsync(client2, 2);

            // get book data
            var books = await client3.Book.GetUserBooksAsync(client1.AuthToken.UserId);

            Assert.AreEqual(books.Count(), 2);
            Assert.IsTrue(books.All(b => b.Id == book1.Id || b.Id == book2.Id));
        }

        [TestMethod]
        public async Task DeleteBook()
        {
            var client = await TestUtil.GetUserClientAsync();
            var book1 = await TestUtil.Books.CreateAsync(client, 0);
            var book2 = await TestUtil.Books.CreateAsync(client, 1);

            // delete
            var mes = await client.Book.DeleteAsync(book1.Id);
            TestUtil.Books.Remove(book1);

            Assert.AreEqual(mes.StatusCode, 200);
            Assert.IsTrue(mes.Message.Contains("succeed"));

            // get deleted book failed
            var e = await Assert.ThrowsExceptionAsync<GunuccoErrorException>(async () =>
            {
                await client.Book.GetAsync(book1.Id);
            });

            // get exists book
            await client.Book.GetAsync(book2.Id);
        }

        [TestMethod]
        public async Task DeleteBook_WithChapter()
        {
            var client = await TestUtil.GetUserClientAsync();
            var book = await TestUtil.Books.CreateAsync(client);
            var chap = await TestUtil.Chapters.CreateAsync(client, 0, book.Id);

            // delete
            var mes = await client.Book.DeleteAsync(book.Id);
            TestUtil.Books.Remove(book);

            Assert.AreEqual(mes.StatusCode, 200);
            Assert.IsTrue(mes.Message.Contains("succeed"));

            // get deleted book failed
            await Assert.ThrowsExceptionAsync<GunuccoErrorException>(async () =>
            {
                await client.Book.GetAsync(book.Id);
            });
            await Assert.ThrowsExceptionAsync<GunuccoErrorException>(async () =>
            {
                await client.Chapter.GetAsync(chap.Id);
            });
        }

        [TestMethod]
        public async Task DeleteBook_Failed_InvalidBookId()
        {
            var client = await TestUtil.GetUserClientAsync();
            var book = await TestUtil.Books.CreateAsync(client);

            // delete
            var e = await Assert.ThrowsExceptionAsync<GunuccoErrorException>(async () =>
            {
                await client.Book.DeleteAsync(book.Id + 1);
            });

            Assert.AreEqual(e.StatusCode, 404);
            Assert.IsTrue(e.Message.Contains("No such book id found"));
        }

        [TestMethod]
        public async Task DeleteBook_Failed_OtherUserBook()
        {
            var client1 = await TestUtil.GetUserClientAsync(0);
            var client2 = await TestUtil.GetUserClientAsync(1);
            var book2 = await TestUtil.Books.CreateAsync(client2);

            // delete
            var e = await Assert.ThrowsExceptionAsync<GunuccoErrorException>(async () =>
            {
                await client1.Book.DeleteAsync(book2.Id);
            });

            Assert.AreEqual(e.StatusCode, 403);
            Assert.IsTrue(e.Message.ToLower().Contains("permission"));
        }
    }
}
