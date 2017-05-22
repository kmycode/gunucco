using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using GunuccoSharp;
using Gunucco.Entities;
using System.Collections.ObjectModel;
using System.Linq;

namespace GunuccoSharp.Test
{
    static class TestSetting
    {
        #region Settings

        // Is debug mode enabled? (debug = test target server is running on VisualStudio)
        public const bool IsLocalHost = true;

        // Is allow invalid SSL cert?
        public const bool IsAllowInvalidSSLCert = true;

        // server path in debug mode (running on VisualStudio)
        private const string debugServerPath = "http://localhost:51238";

        // server path in not debug mode (server on VM or real server)
        private const string serverPath = "https://gunucco.net";

        #endregion

        public static string ServerPath => IsLocalHost ? debugServerPath : serverPath;
    }

    public class TestUser : User
    {
        public User Original { get; }
        public TestUser(User u)
        {
            this.Description = u.Description;
            this.Id = u.Id;
            this.TextId = u.TextId;
            this.Name = u.Name;
            this.Original = u;
        }
        public TestUser() { }
        public string Password { get; set; }
        public bool IsLoginWhenTestGenerator { get; set; } = true;
    }

    static class TestUtil
    {
        public static readonly TestDataGenerator<TestUser> Users = new TestDataGenerator<TestUser>(
            async (c, u) =>
            {
                var user = new TestUser(await c.User.CreateAsync(u.TextId, u.Password));
                if (u.IsLoginWhenTestGenerator)
                {
                    c.AuthToken = await c.User.Login.WithIdAndPasswordAsync(u.TextId, u.Password);
                }
                return user;
            },
            async (c, u) => await c.User.DeleteAsync(),
            (a, b) => a.Id == b.Id,
            new Collection<TestUser>
            {
                new TestUser
                {
                    TextId = "test",
                    Password = "testaa",
                },
                new TestUser
                {
                    TextId = "test2",
                    Password = "quemtd",
                },
                new TestUser
                {
                    TextId = "test3",
                    Password = "k9ps;s",
                },
            });

        public static readonly TestDataGenerator<Book> Books = new TestDataGenerator<Book>(
            async (c, b) => await c.Book.CreateAsync(b.Name),
            async (c, b) => await c.Book.DeleteAsync(b.Id),
            (a, b) => a.Id == b.Id,
            new Collection<Book>
            {
                new Book
                {
                    Name = "Test book",
                },
                new Book
                {
                    Name = "Mahotsukai ni naritai",
                },
                new Book
                {
                    Name = "Horry Potter",
                },
            });

        public static GunuccoSharpClient GetClient()
        {
            return new GunuccoSharpClient
            {
                ServicePath = TestSetting.ServerPath,
                IsAllowInvalidSSLCert = TestSetting.IsAllowInvalidSSLCert,
            };
        }

        public static async Task<GunuccoSharpClient> GetUserClientAsync(int dataId = 0)
        {
            var client = GetClient();
            await Users.CreateAsync(client, dataId);
            return client;
        }

        public static async Task CleanupAsync(GunuccoSharpClient client = null)
        {
            await Books.CleanAsync(client);
            await Users.CleanAsync(client);
        }
    }

    class TestDataGenerator<T>
    {
        private IEnumerable<T> defaultTestDataTemplates;
        private Collection<Tuple<GunuccoSharpClient, T>> objs = new Collection<Tuple<GunuccoSharpClient, T>>();

        private Func<GunuccoSharpClient, T, Task<T>> createAction;
        private Func<GunuccoSharpClient, T, Task> deleteAction;
        private Func<T, T, bool> comparator;

        public TestDataGenerator(Func<GunuccoSharpClient, T, Task<T>> createAction,
                                Func<GunuccoSharpClient, T, Task> deleteAction,
                                Func<T, T, bool> comparator,
                                IEnumerable<T> templates)
        {
            this.createAction = createAction;
            this.deleteAction = deleteAction;
            this.defaultTestDataTemplates = templates;
            this.comparator = comparator;
        }

        public async Task<T> CreateAsync(GunuccoSharpClient client, int dataId = 0)
        {
            var template = this.defaultTestDataTemplates.ElementAt(dataId);
            return await this.CreateAsync(client, template);
        }

        public async Task<T> CreateAsync(GunuccoSharpClient client, T template)
        {
            var obj = await this.createAction(client, template);
            this.Add(client, obj);
            return obj;
        }

        public void Add(T obj)
        {
            this.objs.Add(new Tuple<GunuccoSharpClient, T>(null, obj));
        }

        public void Add(GunuccoSharpClient client, T obj)
        {
            this.objs.Add(new Tuple<GunuccoSharpClient, T>(client, obj));
        }

        public async Task DeleteAsync(GunuccoSharpClient client, T obj)
        {
            await this.deleteAction(client, obj);
            this.Remove(obj);
        }

        public async Task DeleteAsync(T obj)
        {
            var o = this.Find(obj);
            await this.DeleteAsync(o.Item1, o.Item2);
        }

        public void Remove(T obj)
        {
            var o = this.Find(obj);
            this.objs.Remove(o);
        }

        private Tuple<GunuccoSharpClient, T> Find(T obj)
        {
            return this.objs.Where((o => this.comparator(obj, o.Item2))).SingleOrDefault();
        }

        public async Task CleanAsync(GunuccoSharpClient client = null)
        {
            foreach (var obj in this.objs)
            {
                await this.deleteAction(obj.Item1 ?? client ?? new GunuccoSharpClient(), obj.Item2);
            }
            this.objs.Clear();
        }
    }
}
