using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using Gunucco.Entities;
using System.Security.Cryptography;
using System.Text;
using OpenQA.Selenium;
using System.Linq;

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
        [DataRow("ŽR“cŒN‚ª‰j‚¢‚Å‚¢‚é")]
        [DataRow("a:;bcz7")]
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
            Assert.IsTrue(ex.Error.Message.Contains("too") || ex.Error.Message.Contains("only numbers or"));
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

        [DataTestMethod]
        [DataRow(Scope.Read)]
        [DataRow(Scope.Write)]
        [DataRow(Scope.Read | Scope.Write)]
        [DataRow(Scope.Read | Scope.Write | Scope.WriteUserIdentity)]
        [DataRow(Scope.Read | Scope.Write | Scope.WriteUserIdentity | Scope.WriteUserDangerousIdentity)]
        public async Task LoginWithOauth(Scope scope)
        {
            var user = await TestUtil.Users.CreateAsync(TestUtil.GetClient(), 0);
            var client = TestUtil.GetClient();

            // get code
            var code = await client.User.Login.Oauth.CreateCodeAsync(scope);
            Assert.IsFalse(string.IsNullOrEmpty(code.OauthUri));
            Assert.IsTrue(code.OauthUri.StartsWith("http://") || code.OauthUri.StartsWith("https://"));

            using (var web = new TestBrowser(code.OauthUri))
            {
                // open browser and auth
                var form = web.GetOauthRequestForm();

                // check list
                var list = web.GetOauthScopeList();
                if (scope.HasFlag(Scope.Read))
                {
                    Assert.IsTrue(list.Contains("Read user items"));
                }
                else
                {
                    Assert.IsFalse(list.Contains("Read user items"));
                }
                if (scope.HasFlag(Scope.Write))
                {
                    Assert.IsTrue(list.Contains("Write or delete user items"));
                }
                else
                {
                    Assert.IsFalse(list.Contains("Write or delete user items"));
                }
                if (scope.HasFlag(Scope.WriteUserIdentity))
                {
                    Assert.IsTrue(list.Contains("Write user identity"));
                }
                else
                {
                    Assert.IsFalse(list.Contains("Write user identity"));
                }
                if (scope.HasFlag(Scope.WriteUserDangerousIdentity))
                {
                    Assert.IsTrue(list.Contains("Change user password or delete user"));
                }
                else
                {
                    Assert.IsFalse(list.Contains("Change user password or delete user"));
                }

                // auth
                form.FindElement(By.Name("text_id")).SendKeys("test");
                form.FindElement(By.Name("password")).SendKeys("testaa");
                form.Submit();
                while (!web.Driver.Url.EndsWith("done")) ;
            }

            // get token
            var token = await client.User.Login.Oauth.LoginAsync(code);

            Assert.IsFalse(string.IsNullOrEmpty(token.AccessToken));
            Assert.AreEqual(token.Scope, scope);
            Assert.AreEqual(token.UserId, user.Id);
            Assert.AreEqual(token.UserTextId, "test");
        }

        [DataTestMethod]
        [DataRow(Scope.WebClient)]
        public async Task LoginWithOauth_Failed_InvalidScope(Scope scope)
        {
            var user = await TestUtil.Users.CreateAsync(TestUtil.GetClient(), 0);
            var client = TestUtil.GetClient();

            // get code
            var ex = await Assert.ThrowsExceptionAsync<GunuccoErrorException>(async () =>
            {
                await client.User.Login.Oauth.CreateCodeAsync(scope);
            });

            TestUtil.CheckException(ex, 403, "scope");
        }

        [TestMethod]
        public async Task LoginWithOauth_Failed_NoAuthedCode()
        {
            var user = await TestUtil.Users.CreateAsync(TestUtil.GetClient(), 0);
            var client = TestUtil.GetClient();

            // get code
            var code = await client.User.Login.Oauth.CreateCodeAsync(Scope.Read);
            Assert.IsFalse(string.IsNullOrEmpty(code.OauthUri));
            Assert.IsTrue(code.OauthUri.StartsWith("http://") || code.OauthUri.StartsWith("https://"));

            // get token
            var ex = await Assert.ThrowsExceptionAsync<GunuccoErrorException>(async () =>
            {
                await client.User.Login.Oauth.LoginAsync(code);
            });

            TestUtil.CheckException(ex, 400, "authorized");
        }

        [TestMethod]
        public async Task LoginWithOauth_Failed_InvalidCode()
        {
            var user = await TestUtil.Users.CreateAsync(TestUtil.GetClient(), 0);
            var client = TestUtil.GetClient();

            // get code
            var code = await client.User.Login.Oauth.CreateCodeAsync(Scope.Read);
            Assert.IsFalse(string.IsNullOrEmpty(code.OauthUri));
            Assert.IsTrue(code.OauthUri.StartsWith("http://") || code.OauthUri.StartsWith("https://"));

            // get token
            var ex = await Assert.ThrowsExceptionAsync<GunuccoErrorException>(async () =>
            {
                await client.User.Login.Oauth.LoginAsync(code + "a");
            });

            TestUtil.CheckException(ex, 400, "found");
        }

        [TestMethod]
        public async Task LoginWithOauth_And_DoActionInScope()
        {
            var user = await TestUtil.Users.CreateAsync(TestUtil.GetClient(), 0);
            var client = TestUtil.GetClient();

            // get code
            var code = await client.User.Login.Oauth.CreateCodeAsync(Scope.Write);

            using (var web = new TestBrowser(code.OauthUri))
            {
                // open browser and auth
                var form = web.GetOauthRequestForm();

                // auth
                form.FindElement(By.Name("text_id")).SendKeys("test");
                form.FindElement(By.Name("password")).SendKeys("testaa");
                form.Submit();
                while (!web.Driver.Url.EndsWith("done")) ;
            }

            // get token
            var token = await client.User.Login.Oauth.LoginAsync(code);

            // do action
            await client.Book.CreateAsync("test");
        }

        [TestMethod]
        public async Task LoginWithOauth_And_DoAction_Failed_OutScope()
        {
            var user = await TestUtil.Users.CreateAsync(TestUtil.GetClient(), 0);
            var client = TestUtil.GetClient();

            // get code
            var code = await client.User.Login.Oauth.CreateCodeAsync(Scope.Read);

            using (var web = new TestBrowser(code.OauthUri))
            {
                // open browser and auth
                var form = web.GetOauthRequestForm();

                // auth
                form.FindElement(By.Name("text_id")).SendKeys("test");
                form.FindElement(By.Name("password")).SendKeys("testaa");
                form.Submit();
                while (!web.Driver.Url.EndsWith("done")) ;
            }

            // get token
            var token = await client.User.Login.Oauth.LoginAsync(code);

            // do action
            var ex = await Assert.ThrowsExceptionAsync<GunuccoErrorException>(async () =>
            {
                await client.Book.CreateAsync("test");
            });

            TestUtil.CheckException(ex, 403, "scope");
        }

        [TestMethod]
        public async Task GetUser()
        {
            var client1 = await TestUtil.GetUserClientAsync(0);
            var client2 = await TestUtil.GetUserClientAsync(1);
            var client3 = TestUtil.GetClient();

            // get
            var user1 = await client2.User.GetAsync(client1.AuthToken.UserId);
            var user2 = await client3.User.GetAsync(client1.AuthToken.UserId);

            Assert.IsNotNull(user1);
            Assert.IsNotNull(user2);
            Assert.AreEqual(user1.TextId, client1.AuthToken.UserTextId);
            Assert.AreEqual(user2.TextId, client1.AuthToken.UserTextId);
            Assert.AreEqual(user1.Name, client1.AuthToken.UserTextId.ToLower());
            Assert.AreEqual(user2.Name, client1.AuthToken.UserTextId.ToLower());
        }

        [TestMethod]
        public async Task GetUserWithTextId()
        {
            var client1 = await TestUtil.GetUserClientAsync(0);
            var client2 = await TestUtil.GetUserClientAsync(1);
            var client3 = TestUtil.GetClient();

            // get
            var user1 = await client2.User.GetAsync(client1.AuthToken.UserTextId);
            var user2 = await client3.User.GetAsync(client1.AuthToken.UserTextId);

            Assert.IsNotNull(user1);
            Assert.IsNotNull(user2);
            Assert.AreEqual(user1.Id, client1.AuthToken.UserId);
            Assert.AreEqual(user2.Id, client1.AuthToken.UserId);
            Assert.AreEqual(user1.Name, client1.AuthToken.UserTextId.ToLower());
            Assert.AreEqual(user2.Name, client1.AuthToken.UserTextId.ToLower());
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
            Assert.IsTrue(ex.Error.Message.Contains("scope"));
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
