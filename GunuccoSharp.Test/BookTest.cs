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

        [TestMethod]
        public async Task CreateBook()
        {
            var client = await TestUtil.GetUserClientAsync();

            // create book
            var book = await TestUtil.Books.CreateAsync(client, new Book
            {
                Name = "Test book",
            });

            Assert.IsNotNull(book);
            Assert.AreNotEqual(book.Id, 0);
            Assert.IsTrue(book.Created > DateTime.Now.AddMinutes(-3));
            Assert.AreEqual(book.Name, "Test book");
        }

        [DataTestMethod]
        [DataRow("")]
        [DataRow(null)]
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
            Assert.IsTrue(e.Error.Message.Contains("No book name"));
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
            var books = await client1.Book.GetUserBooksAsync(client3.AuthToken.UserId);

            Assert.AreEqual(books.Count(), 2);
            Assert.IsTrue(books.All(b => b.Id == book1.Id || b.Id == book2.Id));
        }

        [TestMethod]
        public async Task DeleteBook()
        {
            var client = await TestUtil.GetUserClientAsync();
            var book = await TestUtil.Books.CreateAsync(client);

            // delete
            var mes = await client.Book.DeleteAsync(book.Id);
            TestUtil.Books.Remove(book);

            Assert.AreEqual(mes.StatusCode, 200);
            Assert.IsTrue(mes.Message.Contains("succeed"));

            // get deleted book failed
            var e = await Assert.ThrowsExceptionAsync<GunuccoErrorException>(async () =>
            {
                await client.Book.GetAsync(book.Id);
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
