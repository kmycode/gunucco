using Gunucco.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GunuccoSharp.Test
{
    [TestClass]
    public class AuthManagementTest
    {
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

        [TestMethod]
        public async Task GetAuthList()
        {
            var client1 = await TestUtil.GetUserClientAsync(0);
            var client2 = await TestUtil.GetUserClientAsync(1);

            var sessions = await client1.Auth.GetListAsync();
            Assert.AreEqual(sessions.Count(), 1);

            var client3 = TestUtil.GetClient();
            await client3.User.Login.WithIdAndPasswordAsync("test", "testaa");

            sessions = await client1.Auth.GetListAsync();
            Assert.AreEqual(sessions.Count(), 2);
        }

        [TestMethod]
        public async Task DeleteAuthData()
        {
            var client1 = await TestUtil.GetUserClientAsync(0);

            var session = (await client1.Auth.GetListAsync()).Single();

            var client2 = TestUtil.GetClient();
            await client2.User.Login.WithIdAndPasswordAsync("test", "testaa");

            var sessions = await client1.Auth.GetListAsync();
            session = sessions.Single(s => s.IdHash != session.IdHash);

            await client1.Auth.DeleteAsync(session.IdHash);

            var ex = await Assert.ThrowsExceptionAsync<GunuccoErrorException>(async () =>
            {
                await client2.Book.CreateAsync("test");
            });

            TestUtil.CheckException(ex, 403, "scope");

            // check session count
            sessions = await client1.Auth.GetListAsync();
            Assert.AreEqual(sessions.Count(), 1);
        }

        [TestMethod]
        public async Task DeleteAuthData_Self()
        {
            try
            {
                var client = await TestUtil.GetUserClientAsync();

                var session = (await client.Auth.GetListAsync()).Single();
                await client.Auth.DeleteAsync(session.IdHash);

                var ex = await Assert.ThrowsExceptionAsync<GunuccoErrorException>(async () =>
                {
                    await client.Book.CreateAsync("test");
                });

                TestUtil.CheckException(ex, 403, "scope");
            }
            finally
            {
                var client = TestUtil.GetClient();
                await client.User.Login.WithIdAndPasswordAsync("test", "testaa");
                await client.User.DeleteAsync();
                TestUtil.Users.Remove(new TestUser(new User { Id = client.AuthToken.UserId, }));
            }
        }

        [TestMethod]
        public async Task DeleteAuthData_Failed_InvalidSessionIdHash()
        {
            var client1 = await TestUtil.GetUserClientAsync(0);
            var client2 = await TestUtil.GetUserClientAsync(1);

            var session1 = (await client1.Auth.GetListAsync()).Single();

            var ex = await Assert.ThrowsExceptionAsync<GunuccoErrorException>(async () =>
            {
                await client2.Auth.DeleteAsync(session1.IdHash);
            });

            TestUtil.CheckException(ex, 404, "found");
        }
    }
}
