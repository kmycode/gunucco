using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using GunuccoSharp;
using Gunucco.Entities;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

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

        public static readonly TestDataGenerator<Chapter> Chapters = new TestDataGenerator<Chapter>(
            async (c, h, args) => await c.Chapter.CreateAsync(h.Name, (int)args[0]),
            async (c, h) => await c.Chapter.DeleteAsync(h.Id),
            (a, b) => a.Id == b.Id,
            new Collection<Chapter>
            {
                new Chapter
                {
                    Name = "Test Chapter",
                },
                new Chapter
                {
                    Name = "Part.1 Shooll Kill",
                },
                new Chapter
                {
                    Name = "Nyontaka",
                },
            });

        public static readonly TestDataGenerator<ContentMediaPair> TextContents = new TestDataGenerator<ContentMediaPair>(
            async (c, n, args) => await c.Content.CreateTextAsync((int)args[0], n.Content.Text),
            async (c, n) => await c.Content.DeleteAsync(n.Content.Id),
            (a, b) => a.Media?.ContentId == b.Content.Id,
            new Collection<ContentMediaPair>
            {
                new ContentMediaPair
                {
                    Content = new Content
                    {
                        Text = "Wagahai ha neko de aru.",
                    },
                },
                new ContentMediaPair
                {
                    Content = new Content
                    {
                        Text = "Let's war!",
                    },
                },
                new ContentMediaPair
                {
                    Content = new Content
                    {
                        Text = "American legs are too long.",
                    },
                },
            });

        public static readonly TestDataGenerator<ContentMediaPair> HtmlContents = new TestDataGenerator<ContentMediaPair>(
            async (c, n, args) => await c.Content.CreateHtmlAsync((int)args[0], n.Content.Text),
            async (c, n) => await c.Content.DeleteAsync(n.Content.Id),
            (a, b) => a.Media?.ContentId == b.Content.Id,
            new Collection<ContentMediaPair>
            {
                new ContentMediaPair
                {
                    Content = new Content
                    {
                        Text = "<span>Wagahai ha neko de aru.</span>",
                    },
                },
                new ContentMediaPair
                {
                    Content = new Content
                    {
                        Text = "<div>Let's war!</div>",
                    },
                },
                new ContentMediaPair
                {
                    Content = new Content
                    {
                        Text = "American legs are <b>too</b> long.",
                    },
                },
            });

        public static readonly TestDataGenerator<ContentMediaPair> ImageContents = new TestDataGenerator<ContentMediaPair>(
            async (c, n, args) =>
            {
                var uri = n.Content.Text;
                n.Content.Text = string.Empty;
                var result = await CreateImageContentAsync(c, (int)args[0], uri);
                n.Content.Text = uri;
                return result;
            },
            async (c, n) => await c.Content.DeleteAsync(n.Content.Id),
            (a, b) => a.Media.Id == b.Content.Id,
            new Collection<ContentMediaPair>
            {
                new ContentMediaPair
                {
                    Content = new Content
                    {
                        Text = "..\\..\\..\\TestFiles\\1.png",
                    },
                },
                new ContentMediaPair
                {
                    Content = new Content
                    {
                        Text = "..\\..\\..\\TestFiles\\2.png",
                    },
                },
                new ContentMediaPair
                {
                    Content = new Content
                    {
                        Text = "..\\..\\..\\TestFiles\\3.png",
                    },
                },
            });

        public static async Task<ContentMediaPair> CreateImageContentAsync(GunuccoSharpClient client, int chapterId, string fileName, MediaExtension ex = MediaExtension.Png)
        {
            using (var stream = new FileStream(fileName, FileMode.Open))
            {
                return await client.Content.CreateImageAsync(chapterId, ex, stream);
            }
        }

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
            // await TextContents.CleanAsync(client);
            // await ImageContents.CleanAsync(client);
            // await Chapters.CleanAsync(client);
            // await Books.CleanAsync(client);
            await Users.CleanAsync(client);
        }

        public static void CheckException(GunuccoErrorException ex, int code, params string[] contains)
        {
            Assert.IsNotNull(ex);
            Assert.AreEqual(ex.StatusCode, code);
            foreach (var str in contains)
            {
                Assert.IsTrue(ex.Error.Message.Contains(str));
            }
        }

        public static void CheckException(ApiMessage ex, int code, params string[] contains)
        {
            CheckException(new GunuccoErrorException(ex) { StatusCode = ex.StatusCode, }, code, contains);
        }
    }

    class TestDataGenerator<T>
    {
        private IEnumerable<T> defaultTestDataTemplates;
        private Collection<Tuple<GunuccoSharpClient, T>> objs = new Collection<Tuple<GunuccoSharpClient, T>>();

        private Func<GunuccoSharpClient, T, object[], Task<T>> createAction;
        private Func<GunuccoSharpClient, T, Task> deleteAction;
        private Func<T, T, bool> comparator;

        public TestDataGenerator(Func<GunuccoSharpClient, T, Task<T>> createAction,
                                Func<GunuccoSharpClient, T, Task> deleteAction,
                                Func<T, T, bool> comparator,
                                IEnumerable<T> templates)
        {
            this.createAction = (c, t, o) => createAction(c, t);
            this.deleteAction = deleteAction;
            this.defaultTestDataTemplates = templates;
            this.comparator = comparator;
        }

        public TestDataGenerator(Func<GunuccoSharpClient, T, object[], Task<T>> createAction,
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
            return await this.CreateAsync(client, dataId, null);
        }

        public async Task<T> CreateAsync(GunuccoSharpClient client, T template)
        {
            return await this.CreateAsync(client, template, null);
        }

        public async Task<T> CreateAsync(GunuccoSharpClient client, int dataId, params object[] args)
        {
            var template = this.defaultTestDataTemplates.ElementAt(dataId);
            return await this.CreateAsync(client, template, args);
        }

        public async Task<T> CreateAsync(GunuccoSharpClient client, T template, params object[] args)
        {
            var obj = await this.createAction(client, template, args);
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
