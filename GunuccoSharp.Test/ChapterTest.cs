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
    public class ChapterTest
    {
        [TestCleanup]
        public async Task Cleanup()
        {
            await TestUtil.CleanupAsync();
        }

        [DataTestMethod]
        [DataRow("Hello!")]
        [DataRow("")]
        [DataRow(null)]
        [DataRow("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa")]
        public async Task CreateChapter(string name)
        {
            var client = await TestUtil.GetUserClientAsync();

            // create book
            var book = await TestUtil.Books.CreateAsync(client);

            // create chapter
            var chap = await TestUtil.Chapters.CreateAsync(client, new Chapter
            {
                Name = name,
            }, book.Id);

            Assert.IsNotNull(chap);
            Assert.AreNotEqual(chap.Id, 0);
            Assert.AreEqual(chap.BookId, book.Id);
            Assert.AreEqual(chap.Name, name ?? "");
            Assert.AreEqual(chap.PublicRange, PublishRange.Private);
        }

        [DataTestMethod]
        [DataRow("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa")]
        public async Task CreateChapter_Failed_InvalidName(string name)
        {
            var client = await TestUtil.GetUserClientAsync();

            // create book
            var book = await TestUtil.Books.CreateAsync(client);

            // create chapter
            var e = await Assert.ThrowsExceptionAsync<GunuccoErrorException>(async () =>
            {
                await TestUtil.Chapters.CreateAsync(client, new Chapter
                {
                    Name = name,
                }, book.Id);
            });

            Assert.IsNotNull(e.Error);
            Assert.AreEqual(e.Error.StatusCode, 400);
            Assert.IsTrue(e.Error.Message.Contains("too"));
        }

        [TestMethod]
        public async Task CreateChapter_Failed_InvalidBookId()
        {
            var client = await TestUtil.GetUserClientAsync();

            // create book
            var book = await TestUtil.Books.CreateAsync(client);

            // create chapter
            var e = await Assert.ThrowsExceptionAsync<GunuccoErrorException>(async () =>
            {
                await TestUtil.Chapters.CreateAsync(client, 0, book.Id + 1);
            });

            Assert.IsNotNull(e.Error);
            Assert.AreEqual(e.Error.StatusCode, 404);
            Assert.IsTrue(e.Error.Message.Contains("found"));
        }

        [TestMethod]
        public async Task CreateChapter_Failed_OthersBook()
        {
            var client1 = await TestUtil.GetUserClientAsync(0);
            var client2 = await TestUtil.GetUserClientAsync(1);

            // create book
            var book1 = await TestUtil.Books.CreateAsync(client1);

            // create chapter
            var e = await Assert.ThrowsExceptionAsync<GunuccoErrorException>(async () =>
            {
                await TestUtil.Chapters.CreateAsync(client2, 0, book1.Id);
            });

            Assert.IsNotNull(e.Error);
            Assert.AreEqual(e.Error.StatusCode, 403);
            Assert.IsTrue(e.Error.Message.Contains("permission"));
        }

        [TestMethod]
        public async Task UpdateChapter()
        {
            var client = await TestUtil.GetUserClientAsync();

            // create book
            var book = await TestUtil.Books.CreateAsync(client);

            // create chapter
            var chap = await TestUtil.Chapters.CreateAsync(client, 0, book.Id);

            // update chapter
            chap.Name += "Zeus";
            chap.PublicRange = PublishRange.All;
            await client.Chapter.UpdateAsync(chap);

            // get newest chapter data
            var newChap = await client.Chapter.GetAsync(chap.Id);

            Assert.IsNotNull(newChap);
            Assert.AreEqual(newChap.Id, chap.Id);
            Assert.AreEqual(newChap.BookId, book.Id);
            Assert.AreEqual(newChap.Name, chap.Name);
            Assert.AreEqual(newChap.PublicRange, PublishRange.All);
        }

        [TestMethod]
        public async Task UpdateChapter_Failed_IdNotFound()
        {
            var client = await TestUtil.GetUserClientAsync();

            // create book
            var book = await TestUtil.Books.CreateAsync(client);

            // create chapter
            var chap = await TestUtil.Chapters.CreateAsync(client, 0, book.Id);

            // update chapter
            chap.Id++;
            var ex = await Assert.ThrowsExceptionAsync<GunuccoErrorException>(async () =>
            {
                await client.Chapter.UpdateAsync(chap);
            });

            Assert.IsNotNull(ex.Error);
            Assert.AreEqual(ex.Error.StatusCode, 404);
            Assert.IsTrue(ex.Error.Message.Contains("found"));
        }

        [TestMethod]
        public async Task UpdateChapter_Failed_OthersId()
        {
            var client1 = await TestUtil.GetUserClientAsync(0);
            var client2 = await TestUtil.GetUserClientAsync(1);

            // create book
            var book1 = await TestUtil.Books.CreateAsync(client1, 0);
            var book2 = await TestUtil.Books.CreateAsync(client2, 1);

            // create chapter
            var chap1 = await TestUtil.Chapters.CreateAsync(client1, 0, book1.Id);
            var chap2 = await TestUtil.Chapters.CreateAsync(client2, 1, book2.Id);

            // update chapter
            var ex = await Assert.ThrowsExceptionAsync<GunuccoErrorException>(async () =>
            {
                await client2.Chapter.UpdateAsync(chap1);
            });

            Assert.IsNotNull(ex.Error);
            Assert.AreEqual(ex.Error.StatusCode, 403);
            Assert.IsTrue(ex.Error.Message.Contains("permission"));
        }

        [TestMethod]
        public async Task UpdateChapter_Failed_DifferentBookId()
        {
            var client = await TestUtil.GetUserClientAsync();

            // create book
            var book1 = await TestUtil.Books.CreateAsync(client, 0);
            var book2 = await TestUtil.Books.CreateAsync(client, 1);

            // create chapter
            var chap = await TestUtil.Chapters.CreateAsync(client, 0, book1.Id);

            // update chapter
            chap.BookId = book2.Id;
            var ex = await Assert.ThrowsExceptionAsync<GunuccoErrorException>(async () =>
            {
                await client.Chapter.UpdateAsync(chap);
            });

            Assert.IsNotNull(ex.Error);
            Assert.AreEqual(ex.Error.StatusCode, 400);
            Assert.IsTrue(ex.Error.Message.Contains("Cannot change book"));
            Assert.IsTrue(ex.Error.Message.Contains(book1.Id.ToString()));
        }

        [DataTestMethod]
        [DataRow("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa")]
        public async Task UpdateChapter_Failed_InvalidChapterName(string name)
        {
            var client = await TestUtil.GetUserClientAsync();

            // create book
            var book = await TestUtil.Books.CreateAsync(client);

            // create chapter
            var chap = await TestUtil.Chapters.CreateAsync(client, 0, book.Id);

            // update chapter
            chap.Name = name;
            var ex = await Assert.ThrowsExceptionAsync<GunuccoErrorException>(async () =>
            {
                await client.Chapter.UpdateAsync(chap);
            });

            Assert.IsNotNull(ex.Error);
            Assert.AreEqual(ex.Error.StatusCode, 400);
            Assert.IsTrue(ex.Error.Message.Contains("too"));
        }

        [TestMethod]
        public async Task UpdateChapter_WithParent()
        {
            var client = await TestUtil.GetUserClientAsync();

            // create book
            var book = await TestUtil.Books.CreateAsync(client);

            // create chapter
            var chap1 = await TestUtil.Chapters.CreateAsync(client, 0, book.Id);
            var chap2 = await TestUtil.Chapters.CreateAsync(client, 1, book.Id);

            // update chapter
            chap2.ParentId = chap1.Id;
            await client.Chapter.UpdateAsync(chap2);

            // get newest chapter data
            var newChap2 = await client.Chapter.GetAsync(chap2.Id);
            var chap1Children = await client.Chapter.GetChildrenAsync(chap1.Id);

            Assert.IsNotNull(newChap2);
            Assert.AreEqual(newChap2.ParentId, chap2.ParentId);
            Assert.AreEqual(chap1Children.Count(), 1);
            Assert.AreEqual(chap1Children.First().Id, chap2.Id);
        }

        [DataTestMethod]
        [DataRow(0)]
        [DataRow(1)]
        [DataRow(2)]
        [DataRow(4)]
        [DataRow(10)]
        public async Task UpdateChapter_Failed_ParentLoop(int loopCount)
        {
            var client = await TestUtil.GetUserClientAsync();

            // create book
            var book = await TestUtil.Books.CreateAsync(client);

            // create chapter
            var chap = await TestUtil.Chapters.CreateAsync(client, 0, book.Id);

            int lastId = chap.Id;
            for (int i = 0; i < loopCount - 1; i++)
            {
                var c = await TestUtil.Chapters.CreateAsync(client, 1, book.Id);
                c.ParentId = lastId;
                await client.Chapter.UpdateAsync(c);
                lastId = c.Id;
            }

            // set loop parent id
            chap.ParentId = lastId;
            var ex = await Assert.ThrowsExceptionAsync<GunuccoErrorException>(async () =>
            {
                await client.Chapter.UpdateAsync(chap);
            });

            TestUtil.CheckException(ex, 400, "parent");
        }

        [TestMethod]
        public async Task UpdateChapter_Failed_ParentNotFound()
        {
            var client = await TestUtil.GetUserClientAsync();

            // create book
            var book = await TestUtil.Books.CreateAsync(client);

            // create chapter
            var chap = await TestUtil.Chapters.CreateAsync(client, 0, book.Id);

            // update chapter
            chap.ParentId = chap.Id + 1;
            var ex = await Assert.ThrowsExceptionAsync<GunuccoErrorException>(async () =>
            {
                await client.Chapter.UpdateAsync(chap);
            });

            Assert.IsNotNull(ex.Error);
            Assert.AreEqual(ex.Error.StatusCode, 404);
            Assert.IsTrue(ex.Error.Message.Contains("parent"));
            Assert.IsTrue(ex.Error.Message.Contains("found"));
        }

        [TestMethod]
        public async Task UpdateChapter_Failed_Parent_Belongs_OtherBook()
        {
            var client = await TestUtil.GetUserClientAsync();

            // create book
            var book1 = await TestUtil.Books.CreateAsync(client, 0);
            var book2 = await TestUtil.Books.CreateAsync(client, 1);

            // create chapter
            var chap1 = await TestUtil.Chapters.CreateAsync(client, 0, book1.Id);
            var chap2 = await TestUtil.Chapters.CreateAsync(client, 1, book2.Id);

            // update chapter
            chap2.ParentId = chap1.Id;
            var ex = await Assert.ThrowsExceptionAsync<GunuccoErrorException>(async () =>
            {
                await client.Chapter.UpdateAsync(chap2);
            });

            Assert.IsNotNull(ex.Error);
            Assert.AreEqual(ex.Error.StatusCode, 400);
            Assert.IsTrue(ex.Error.Message.Contains("parent"));
            Assert.IsTrue(ex.Error.Message.Contains("different"));
        }

        [TestMethod]
        public async Task UpdateChapter_WithChangeOrder()
        {
            var client = await TestUtil.GetUserClientAsync();

            // create book
            var book = await TestUtil.Books.CreateAsync(client);

            // create chapter
            var chap1 = await TestUtil.Chapters.CreateAsync(client, 0, book.Id);
            chap1.Order = 5;
            await client.Chapter.UpdateAsync(chap1);
            var chap2 = await TestUtil.Chapters.CreateAsync(client, 1, book.Id);
            chap2.Order = 2;
            await client.Chapter.UpdateAsync(chap2);
            var chap3 = await TestUtil.Chapters.CreateAsync(client, 2, book.Id);
            chap3.Order = 3;
            chap3.ParentId = chap2.Id;
            await client.Chapter.UpdateAsync(chap3);

            // get chapters
            var chapters = await client.Book.GetChaptersAsync(book.Id);
            var chapters_root = await client.Book.GetRootChaptersAsync(book.Id);

            Assert.AreEqual(chapters.Count(), 3);
            Assert.AreEqual(chapters.First().Order, 2);
            Assert.AreEqual(chapters.First().Id, chap2.Id);
            Assert.AreEqual(chapters.ElementAt(1).Order, 3);
            Assert.AreEqual(chapters.ElementAt(1).Id, chap3.Id);
            Assert.AreEqual(chapters.ElementAt(2).Order, 5);
            Assert.AreEqual(chapters.ElementAt(2).Id, chap1.Id);

            Assert.AreEqual(chapters_root.Count(), 2);
            Assert.AreEqual(chapters_root.First().Order, 2);
            Assert.AreEqual(chapters_root.First().Id, chap2.Id);
            Assert.AreEqual(chapters_root.ElementAt(1).Order, 5);
            Assert.AreEqual(chapters_root.ElementAt(1).Id, chap1.Id);
        }

        [DataTestMethod]
        [DataRow(PublishRange.Private)]
        [DataRow(PublishRange.UserOnly)]
        [DataRow(PublishRange.All)]
        public async Task GetChapter_Mine(PublishRange range)
        {
            var client = await TestUtil.GetUserClientAsync();

            // create book
            var book = await TestUtil.Books.CreateAsync(client);

            // create chapter
            var chap = await TestUtil.Chapters.CreateAsync(client, 0, book.Id);

            // update chapter
            chap.PublicRange = range;
            await client.Chapter.UpdateAsync(chap);

            // get newest chapter data
            var newChap = await client.Chapter.GetAsync(chap.Id);
        }

        [DataTestMethod]
        [DataRow(PublishRange.All, false)]
        [DataRow(PublishRange.All, true)]
        [DataRow(PublishRange.UserOnly, true)]
        public async Task GetChapter_Others(PublishRange range, bool doLogin)
        {
            var client1 = await TestUtil.GetUserClientAsync(0);
            var client2 = await TestUtil.GetUserClientAsync(1);
            var client3 = TestUtil.GetClient();

            // create book
            var book = await TestUtil.Books.CreateAsync(client1);

            // create chapter
            var chap = await TestUtil.Chapters.CreateAsync(client1, 0, book.Id);

            // update chapter
            chap.PublicRange = range;
            await client1.Chapter.UpdateAsync(chap);

            // get newest chapter data
            var newChap_1 = await client1.Chapter.GetAsync(chap.Id);
            Chapter newChap_2;
            if (doLogin)
            {
                newChap_2 = await client2.Chapter.GetAsync(chap.Id);
            }
            else
            {
                newChap_2 = await client3.Chapter.GetAsync(chap.Id);
            }

            Assert.AreEqual(newChap_1.Id, newChap_2.Id);
            Assert.AreEqual(newChap_1.Name, newChap_2.Name);
        }

        [TestMethod]
        public async Task GetChapter_Failed_InvalidId()
        {
            var client = await TestUtil.GetUserClientAsync();

            // get chapter
            var ex = await Assert.ThrowsExceptionAsync<GunuccoErrorException>(async () =>
            {
                await client.Chapter.GetAsync(0);
            });

            Assert.IsNotNull(ex.Error);
            Assert.AreEqual(ex.Error.StatusCode, 404);
            Assert.IsTrue(ex.Error.Message.Contains("found"));
        }

        [DataTestMethod]
        [DataRow(PublishRange.Private, true)]
        [DataRow(PublishRange.UserOnly, false)]
        public async Task GetChapter_Failed_PermissionDenied(PublishRange range, bool doLogin)
        {
            var client1 = await TestUtil.GetUserClientAsync(0);
            var client2 = await TestUtil.GetUserClientAsync(1);
            var client3 = TestUtil.GetClient();

            // create book
            var book = await TestUtil.Books.CreateAsync(client1);

            // create chapter
            var chap = await TestUtil.Chapters.CreateAsync(client1, 0, book.Id);

            // update chapter
            chap.PublicRange = range;
            await client1.Chapter.UpdateAsync(chap);

            // get newest chapter data
            var ex = await Assert.ThrowsExceptionAsync<GunuccoErrorException>(async () =>
            {
                if (doLogin)
                {
                    await client2.Chapter.GetAsync(chap.Id);
                }
                else
                {
                    await client3.Chapter.GetAsync(chap.Id);
                }
            });

            TestUtil.CheckException(ex, 403, "permission");
        }

        [DataTestMethod]
        [DataRow(1, 1)]
        [DataRow(3, 3)]
        [DataRow(4, 3)]
        [DataRow(0, 0)]
        [DataRow(1, 0)]
        public async Task GetChildChapters(int chapterCount, int publicChapterCount)
        {
            var client1 = await TestUtil.GetUserClientAsync(0);
            var client2 = await TestUtil.GetUserClientAsync(1);

            // create book
            var book = await TestUtil.Books.CreateAsync(client1);

            // create root chapter
            var chap = await TestUtil.Chapters.CreateAsync(client1, 0, book.Id);
            chap.PublicRange = PublishRange.All;
            await client1.Chapter.UpdateAsync(chap);

            // create chapter
            for (int i = 0; i < chapterCount; i++)
            {
                var child = await TestUtil.Chapters.CreateAsync(client1, 0, book.Id);
                child.ParentId = chap.Id;
                if (i < publicChapterCount)
                {
                    child.PublicRange = PublishRange.All;
                }
                await client1.Chapter.UpdateAsync(child);
            }

            // get children chapters
            var children = await client2.Chapter.GetChildrenAsync(chap.Id);

            Assert.IsNotNull(children);
            Assert.AreEqual(children.Count(), publicChapterCount);
            Assert.IsTrue(children.All(c => c.ParentId == chap.Id));
        }

        [TestMethod]
        public async Task GetChildChapters_Failed_PermissionDenied()
        {
            var client1 = await TestUtil.GetUserClientAsync(0);
            var client2 = await TestUtil.GetUserClientAsync(1);

            // create book
            var book = await TestUtil.Books.CreateAsync(client1);

            // create root chapter
            var chap = await TestUtil.Chapters.CreateAsync(client1, 0, book.Id);
            Assert.AreEqual(chap.PublicRange, PublishRange.Private);

            // get children chapters
            var ex = await Assert.ThrowsExceptionAsync<GunuccoErrorException>(async () =>
            {
                await client2.Chapter.GetChildrenAsync(chap.Id);
            });

            TestUtil.CheckException(ex, 403, "permission");
        }

        [TestMethod]
        public async Task GetChildChapters_Failed_InvalidId()
        {
            var client = await TestUtil.GetUserClientAsync();

            // create book
            var book = await TestUtil.Books.CreateAsync(client);

            // create root chapter
            var chap = await TestUtil.Chapters.CreateAsync(client, 0, book.Id);

            // get children chapters
            var ex = await Assert.ThrowsExceptionAsync<GunuccoErrorException>(async () =>
            {
                await client.Chapter.GetChildrenAsync(chap.Id + 1);
            });

            TestUtil.CheckException(ex, 404, "found");
        }

        [TestMethod]
        public async Task DeleteChapter()
        {
            var client = await TestUtil.GetUserClientAsync();

            // create book
            var book = await TestUtil.Books.CreateAsync(client);

            // create chapter
            var chap1 = await TestUtil.Chapters.CreateAsync(client, 0, book.Id);
            var chap2 = await TestUtil.Chapters.CreateAsync(client, 1, book.Id);

            // delete chapter
            var mes = await client.Chapter.DeleteAsync(chap1.Id);
            TestUtil.Chapters.Remove(chap1);

            Assert.AreEqual(mes.StatusCode, 200);
            Assert.IsTrue(mes.Message.Contains("succeed"));

            // get deleted chapter failed
            var e = await Assert.ThrowsExceptionAsync<GunuccoErrorException>(async () =>
            {
                await client.Chapter.GetAsync(chap1.Id);
            });

            // get exists chapter
            await client.Chapter.GetAsync(chap2.Id);
        }

        [DataTestMethod]
        [DataRow(1)]
        [DataRow(3)]
        public async Task DeleteChapter_WithContent(int contentCount)
        {
            var client = await TestUtil.GetUserClientAsync();

            // create book
            var book = await TestUtil.Books.CreateAsync(client);

            // create chapter
            var chap1 = await TestUtil.Chapters.CreateAsync(client, 0, book.Id);
            var chap2 = await TestUtil.Chapters.CreateAsync(client, 1, book.Id);

            // create content
            int checkContentId = 0;
            for (int i = 0; i < contentCount; i++)
            {
                var cont = await TestUtil.TextContents.CreateAsync(client, i, chap1.Id);
                checkContentId = cont.Content.Id;
            }

            // delete chapter
            var mes = await client.Chapter.DeleteAsync(chap1.Id);
            TestUtil.Chapters.Remove(chap1);

            Assert.AreEqual(mes.StatusCode, 200);
            Assert.IsTrue(mes.Message.Contains("succeed"));

            // get deleted chapter failed
            var e = await Assert.ThrowsExceptionAsync<GunuccoErrorException>(async () =>
            {
                await client.Content.GetAsync(checkContentId);
            });

            // get exists chapter
            await client.Chapter.GetAsync(chap2.Id);
        }

        [TestMethod]
        public async Task DeleteChapter_WithChildren()
        {
            var client = await TestUtil.GetUserClientAsync();

            // create book
            var book = await TestUtil.Books.CreateAsync(client);

            // create chapter
            var chap1 = await TestUtil.Chapters.CreateAsync(client, 0, book.Id);
            var chap2 = await TestUtil.Chapters.CreateAsync(client, 1, book.Id);

            // set parent
            chap2.ParentId = chap1.Id;
            await client.Chapter.UpdateAsync(chap2);

            // delete chapter
            var mes = await client.Chapter.DeleteAsync(chap1.Id);
            TestUtil.Chapters.Remove(chap1);
            TestUtil.Chapters.Remove(chap2);

            Assert.AreEqual(mes.StatusCode, 200);
            Assert.IsTrue(mes.Message.Contains("succeed"));

            // get deleted chapter failed
            await Assert.ThrowsExceptionAsync<GunuccoErrorException>(async () =>
            {
                await client.Chapter.GetAsync(chap1.Id);
            });
            await Assert.ThrowsExceptionAsync<GunuccoErrorException>(async () =>
            {
                await client.Chapter.GetAsync(chap2.Id);
            });
        }

        [TestMethod]
        public async Task DeleteChapter_Failed_InvalidId()
        {
            var client = await TestUtil.GetUserClientAsync();

            // create book
            var book = await TestUtil.Books.CreateAsync(client);

            // create root chapter
            var chap = await TestUtil.Chapters.CreateAsync(client, 0, book.Id);

            // get children chapters
            var ex = await Assert.ThrowsExceptionAsync<GunuccoErrorException>(async () =>
            {
                await client.Chapter.DeleteAsync(chap.Id + 1);
            });

            TestUtil.CheckException(ex, 404, "found");
        }

        [TestMethod]
        public async Task DeleteChapter_Failed_PermissionDenied()
        {
            var client1 = await TestUtil.GetUserClientAsync(0);
            var client2 = await TestUtil.GetUserClientAsync(1);

            // create book
            var book = await TestUtil.Books.CreateAsync(client1);

            // create root chapter
            var chap = await TestUtil.Chapters.CreateAsync(client1, 0, book.Id);
            Assert.AreEqual(chap.PublicRange, PublishRange.Private);

            // get children chapters
            var ex = await Assert.ThrowsExceptionAsync<GunuccoErrorException>(async () =>
            {
                await client2.Chapter.DeleteAsync(chap.Id);
            });

            TestUtil.CheckException(ex, 403, "permission");
        }
    }
}
