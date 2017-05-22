using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using Gunucco.Entities;
using System.Security.Cryptography;
using System.Text;

namespace GunuccoSharp.Test
{
    [TestClass]
    public class UserTest
    {
        private const string userId = "test";
        private const string userPassword = "testaa";

        [TestInitialize]
        public async Task Initialize()
        {
            await TestUtil.CleanupAsync();
        }

        [TestCleanup]
        public async Task CleanUp()
        {
            await TestUtil.CleanupAsync();
        }

        [DataTestMethod]
        [DataRow(userId)]
        [DataRow("abcdef")]
        public async Task CreateUser(string id)
        {
            // create
            var client = TestUtil.GetClient();
            var user = await TestUtil.Users.CreateAsync(client, new TestUser
            {
                TextId = id,
                Password = "aaaeee",
            });

            Assert.IsNotNull(user);
            Assert.AreEqual(user.TextId, id);
        }

        [DataTestMethod]
        [DataRow("")]
        [DataRow(null)]
        [DataRow("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa")]
        public async Task CreateUser_Failed_InvalidId(string id)
        {
            var client = TestUtil.GetClient();

            var ex = await Assert.ThrowsExceptionAsync<GunuccoErrorException>(async () =>
            {
                await TestUtil.Users.CreateAsync(client, new TestUser
                {
                    TextId = id,
                    Password = "aaaeee",
                });
            });
            Assert.IsTrue(ex.Error.Message.Contains("too"));
        }

        [DataTestMethod]
        [DataRow("")]
        [DataRow(null)]
        [DataRow("s")]
        [DataRow("abcde")]
        public async Task CreateUser_Failed_NoEnoughPassword(string pass)
        {
            var client = TestUtil.GetClient();

            var ex = await Assert.ThrowsExceptionAsync<GunuccoErrorException>(async () =>
            {
                await TestUtil.Users.CreateAsync(client, new TestUser
                {
                    TextId = "testaa",
                    Password = pass,
                });
            });
            Assert.IsTrue(ex.Error.Message.Contains("too short"));
        }

        [TestMethod]
        public async Task CreateUser_Failed_AlreadyExistsId()
        {
            var client = TestUtil.GetClient();

            // create
            await TestUtil.Users.CreateAsync(client, new TestUser
            {
                TextId = "testaa",
                Password = "rerere",
            });

            var ex = await Assert.ThrowsExceptionAsync<GunuccoErrorException>(async () =>
            {
                await TestUtil.Users.CreateAsync(client, new TestUser
                {
                    TextId = "testaa",
                    Password = "rerere",
                });
            });
            Assert.IsTrue(ex.Error.Message.Contains("Existing"));
        }

        [TestMethod]
        public async Task LoginWithIdAndPassword()
        {
            var client = TestUtil.GetClient();

            // create
            await TestUtil.Users.CreateAsync(client, new TestUser
            {
                TextId = "testaa",
                Password = "rerere",
                IsLoginWhenTestGenerator = false,
            });

            var token = await client.User.Login.WithIdAndPasswordAsync("testaa", "rerere");
            
            Assert.IsNotNull(token);
            Assert.AreEqual(token.UserTextId, "testaa");
            Assert.AreNotEqual(token.UserId, 0);
        }

        [DataTestMethod]
        [DataRow("")]
        [DataRow(userPassword + "q")]
        [DataRow(null)]
        public async Task LoginWithIdAndPassword_Failed_InvalidPassword(string pass)
        {
            var client = TestUtil.GetClient();
            var testClient = TestUtil.GetClient();

            // create
            await TestUtil.Users.CreateAsync(client, new TestUser
            {
                TextId = "testaa",
                Password = "rerere",
            });

            var exception = await Assert.ThrowsExceptionAsync<GunuccoErrorException>(async () =>
            {
                await testClient.User.Login.WithIdAndPasswordAsync("testaa", pass);
            });

            Assert.AreEqual(exception.StatusCode, 400);
            Assert.IsNotNull(exception.Error);
            Assert.AreEqual(exception.Error.StatusCode, 400);
            Assert.IsTrue(exception.Error.Message.Contains("Login failed"));
        }

        [DataTestMethod]
        [DataRow("")]
        [DataRow(userId + "logom")]
        [DataRow(null)]
        public async Task LoginWithIdAndPassword_Failed_InvalidUserId(string id)
        {
            var client = TestUtil.GetClient();
            var testClient = TestUtil.GetClient();

            // create
            await TestUtil.Users.CreateAsync(client, new TestUser
            {
                TextId = "testaa",
                Password = "rerere",
            });

            var exception = await Assert.ThrowsExceptionAsync<GunuccoErrorException>(async () =>
            {
                var token = await testClient.User.Login.WithIdAndPasswordAsync(id, "rerere");
            });

            Assert.AreEqual(exception.StatusCode, 400);
            Assert.IsNotNull(exception.Error);
            Assert.AreEqual(exception.Error.StatusCode, 400);
            Assert.IsTrue(exception.Error.Message.Contains("No such user"));
        }

        [TestMethod]
        public async Task DeleteUser()
        {
            var client = TestUtil.GetClient();

            // create
            var user = await TestUtil.Users.CreateAsync(client, 0);

            // delete
            var mes = await client.User.DeleteAsync();
            TestUtil.Users.Remove(user);

            Assert.IsNotNull(mes);
            Assert.AreEqual(mes.StatusCode, 200);
            Assert.IsTrue(mes.Message.Contains("succeed"));
        }

        [TestMethod]
        public async Task DeleteUser_Failed_NoAuth()
        {
            var client = TestUtil.GetClient();

            var ex = await Assert.ThrowsExceptionAsync<GunuccoErrorException>(async () =>
            {
                await client.User.DeleteAsync();
            });
            Assert.IsTrue(ex.Error.Message.Contains("Login failed"));
        }

        [TestMethod]
        public async Task DeleteUser_WithBooks()
        {
            var client1 = TestUtil.GetClient();
            var client2 = await TestUtil.GetUserClientAsync(1);

            var user1 = await TestUtil.Users.CreateAsync(client1, 0);

            var book1 = await TestUtil.Books.CreateAsync(client1, 0);
            var book2 = await TestUtil.Books.CreateAsync(client2, 1);

            // delete user
            await TestUtil.Users.DeleteAsync(user1);
            TestUtil.Books.Remove(book1);

            // try get deleted book
            var ex = await Assert.ThrowsExceptionAsync<GunuccoErrorException>(async () =>
            {
                await client2.Book.GetAsync(book1.Id);
            });

            Assert.AreEqual(ex.Error.StatusCode, 404);
            Assert.IsTrue(ex.Error.Message.Contains("found"));

            // try get exists book
            var book2_get = await client2.Book.GetAsync(book2.Id);

            Assert.AreEqual(book2_get.Id, book2.Id);
            Assert.AreEqual(book2_get.Name, book2.Name);
        }
    }
}
