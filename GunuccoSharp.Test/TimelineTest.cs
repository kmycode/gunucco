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
    public class TimelineTest
    {
        [TestCleanup]
        public async Task Cleanup()
        {
            await TestUtil.CleanupAsync();
        }

        [TestMethod]
        public async Task Timeline_CreateBook()
        {
            var client = await TestUtil.GetUserClientAsync();

            // create book
            var book = await TestUtil.Books.CreateAsync(client);

            // get timeline
            var tl = await client.Timeline.GetLocalAsync();
            var tlbook = tl.Where(i => i.TimelineItem.TargetType == TargetType.Book).SingleOrDefault(i => i.TimelineItem.TargetId == book.Id);

            Assert.IsNotNull(tlbook);
            Assert.IsNotNull(tlbook.Book);
            Assert.IsNull(tlbook.Chapter);
            Assert.IsNull(tlbook.ContentMediaPair);
            Assert.AreEqual(tlbook.Book.Name, book.Name);
            Assert.AreEqual(tlbook.TimelineItem.TargetAction, TargetAction.Create);

            Assert.IsNotNull(tlbook.User);
            Assert.AreEqual(tlbook.User.Id, client.AuthToken.UserId);
        }

        [TestMethod]
        public async Task Timeline_CreateBookAndChapter()
        {
            var client = await TestUtil.GetUserClientAsync();

            // create book
            var book = await TestUtil.Books.CreateAsync(client);

            // create chapter
            var chap = await TestUtil.Chapters.CreateAsync(client, 0, book.Id);

            // get timeline
            var tl = await client.Timeline.GetLocalAsync();
            var tlbook = tl.Where(i => i.TimelineItem.TargetType == TargetType.Book).SingleOrDefault(i => i.TimelineItem.TargetId == book.Id);
            var tlchap = tl.Where(i => i.TimelineItem.TargetType == TargetType.Chapter).SingleOrDefault(i => i.TimelineItem.TargetId == chap.Id);

            Assert.IsNotNull(tlbook);
            Assert.AreEqual(tlbook.TimelineItem.TargetAction, TargetAction.Create);

            Assert.IsNotNull(tlchap);
            Assert.IsNotNull(tlchap.Chapter);
            Assert.IsNotNull(tlchap.Book);
            Assert.AreEqual(tlchap.Book.Name, book.Name);
            Assert.IsNull(tlchap.ContentMediaPair);
            Assert.AreEqual(tlchap.Chapter.Name, chap.Name);
            Assert.AreEqual(tlchap.TimelineItem.TargetAction, TargetAction.Create);

            Assert.IsNotNull(tlbook.User);
            Assert.IsNotNull(tlchap.User);
            Assert.AreEqual(tlchap.User.Id, client.AuthToken.UserId);
        }

        [TestMethod]
        public async Task Timeline_CreateBookAndChapterAndTextContent()
        {
            var client = await TestUtil.GetUserClientAsync();

            // create book
            var book = await TestUtil.Books.CreateAsync(client);

            // create chapter
            var chap = await TestUtil.Chapters.CreateAsync(client, 0, book.Id);

            // create text content
            var cont = await TestUtil.TextContents.CreateAsync(client, 0, chap.Id);

            // get timeline
            var tl = await client.Timeline.GetLocalAsync();
            var tlbook = tl.Where(i => i.TimelineItem.TargetType == TargetType.Book).SingleOrDefault(i => i.TimelineItem.TargetId == book.Id);
            var tlchap = tl.Where(i => i.TimelineItem.TargetType == TargetType.Chapter).SingleOrDefault(i => i.TimelineItem.TargetId == chap.Id);
            var tlcont = tl.Where(i => i.TimelineItem.TargetType == TargetType.Content).SingleOrDefault(i => i.TimelineItem.TargetId == cont.Content.Id);

            Assert.IsNotNull(tlbook);
            Assert.AreEqual(tlbook.TimelineItem.TargetAction, TargetAction.Create);

            Assert.IsNotNull(tlchap);
            Assert.AreEqual(tlchap.TimelineItem.TargetAction, TargetAction.Create);

            Assert.IsNotNull(tlcont);
            Assert.IsNotNull(tlcont.ContentMediaPair);
            Assert.IsNotNull(tlcont.Book);
            Assert.AreEqual(tlcont.Book.Name, book.Name);
            Assert.IsNotNull(tlcont.Chapter);
            Assert.AreEqual(tlcont.ContentMediaPair.Content.Text, cont.Content.Text);
            Assert.AreEqual(tlcont.TimelineItem.TargetAction, TargetAction.Create);

            Assert.IsNotNull(tlbook.User);
            Assert.IsNotNull(tlchap.User);
            Assert.IsNotNull(tlcont.User);
            Assert.AreEqual(tlcont.User.Id, client.AuthToken.UserId);
        }

        [TestMethod]
        public async Task Timeline_CreateBookAndChapterAndImageContent()
        {
            var client = await TestUtil.GetUserClientAsync();

            // create book
            var book = await TestUtil.Books.CreateAsync(client);

            // create chapter
            var chap = await TestUtil.Chapters.CreateAsync(client, 0, book.Id);

            // create image content
            var cont = await TestUtil.ImageContents.CreateAsync(client, 0, chap.Id);

            // get timeline
            var tl = await client.Timeline.GetLocalAsync();
            var tlbook = tl.Where(i => i.TimelineItem.TargetType == TargetType.Book).SingleOrDefault(i => i.TimelineItem.TargetId == book.Id);
            var tlchap = tl.Where(i => i.TimelineItem.TargetType == TargetType.Chapter).SingleOrDefault(i => i.TimelineItem.TargetId == chap.Id);
            var tlcont = tl.Where(i => i.TimelineItem.TargetType == TargetType.Content).SingleOrDefault(i => i.TimelineItem.TargetId == cont.Content.Id);

            Assert.IsNotNull(tlbook);
            Assert.AreEqual(tlbook.TimelineItem.TargetAction, TargetAction.Create);

            Assert.IsNotNull(tlchap);
            Assert.AreEqual(tlchap.TimelineItem.TargetAction, TargetAction.Create);

            Assert.IsNotNull(tlcont);
            Assert.IsNotNull(tlcont.ContentMediaPair);
            Assert.IsNotNull(tlcont.ContentMediaPair.Media);
            Assert.IsNotNull(tlcont.Book);
            Assert.IsNotNull(tlcont.Chapter);
            Assert.IsFalse(string.IsNullOrEmpty(tlcont.ContentMediaPair.Media.Uri));
            Assert.IsTrue(tlcont.ContentMediaPair.Media.Uri.StartsWith("http"));
            Assert.AreEqual(tlcont.TimelineItem.TargetAction, TargetAction.Create);

            Assert.IsNotNull(tlbook.User);
            Assert.IsNotNull(tlchap.User);
            Assert.IsNotNull(tlcont.User);
            Assert.AreEqual(tlcont.User.Id, client.AuthToken.UserId);
        }

        [TestMethod]
        public async Task Timeline_CreateChildChapter()
        {
            var client = await TestUtil.GetUserClientAsync();

            // create book
            var book = await TestUtil.Books.CreateAsync(client);

            // create chapter
            var chap1 = await TestUtil.Chapters.CreateAsync(client, 0, book.Id);
            var chap2 = await TestUtil.Chapters.CreateAsync(client, 1, book.Id);
            chap2.ParentId = chap1.Id;
            await client.Chapter.UpdateAsync(chap2);

            // get timeline
            var tl = await client.Timeline.GetLocalAsync();
            var tlbook = tl.Where(i => i.TimelineItem.TargetType == TargetType.Book).SingleOrDefault(i => i.TimelineItem.TargetId == book.Id);
            var tlchaps = tl.Where(i => i.TimelineItem.TargetType == TargetType.Chapter).Where(i => i.TimelineItem.TargetId == chap2.Id);
            var tlchap2 = tlchaps.FirstOrDefault();
            var tlchap1 = tl.Where(i => i.TimelineItem.TargetType == TargetType.Chapter).SingleOrDefault(i => i.TimelineItem.TargetId == chap1.Id);

            Assert.IsNotNull(tlbook);
            Assert.AreEqual(tlbook.TimelineItem.TargetAction, TargetAction.Create);

            Assert.IsNotNull(tlchap1);
            Assert.AreEqual(tlchap1.TimelineItem.TargetAction, TargetAction.Create);

            Assert.IsNotNull(tlchap2);
            Assert.AreEqual(tlchap2.Chapter.ParentId, chap1.Id);
            Assert.AreEqual(tlchap2.TimelineItem.TargetAction, TargetAction.Update);

            Assert.IsNotNull(tlbook.User);
            Assert.IsNotNull(tlchap2.User);
            Assert.AreEqual(tlchap2.User.Id, client.AuthToken.UserId);
        }

        [TestMethod]
        public async Task Timeline_UpdateBook()
        {
            var client = await TestUtil.GetUserClientAsync();

            // create book
            var book = await TestUtil.Books.CreateAsync(client);

            // update book
            book.Name = "Hello book";
            await client.Book.UpdateAsync(book);

            // get timeline
            var tl = await client.Timeline.GetLocalAsync();
            var tlbooks = tl.Where(i => i.TimelineItem.TargetType == TargetType.Book).Where(i => i.TimelineItem.TargetId == book.Id);
            var tlbook = tlbooks.FirstOrDefault();

            Assert.IsNotNull(tlbook);
            Assert.AreEqual(tlbooks.Count(), 2);
            Assert.IsTrue(tlbooks.Any(i => i.TimelineItem.TargetAction == TargetAction.Create));
            Assert.AreEqual(tlbook.TimelineItem.TargetAction, TargetAction.Update);
            Assert.AreEqual(tlbook.Book.Name, "Hello book");
            Assert.IsNull(tlbook.Chapter);
            Assert.IsNull(tlbook.ContentMediaPair);

            Assert.IsNotNull(tlbook.User);
            Assert.AreEqual(tlbook.User.Id, client.AuthToken.UserId);
        }

        [TestMethod]
        public async Task Timeline_UpdateChapter()
        {
            var client = await TestUtil.GetUserClientAsync();

            // create book
            var book = await TestUtil.Books.CreateAsync(client);

            // create chapter
            var chap = await TestUtil.Chapters.CreateAsync(client, 0, book.Id);

            // update chapter
            chap.Name = "Hello chapter";
            await client.Chapter.UpdateAsync(chap);

            // get timeline
            var tl = await client.Timeline.GetLocalAsync();
            var tlchaps = tl.Where(i => i.TimelineItem.TargetType == TargetType.Chapter).Where(i => i.TimelineItem.TargetId == chap.Id);
            var tlchap = tlchaps.FirstOrDefault();

            Assert.IsNotNull(tlchap);
            Assert.AreEqual(tlchaps.Count(), 2);
            Assert.IsTrue(tlchaps.Any(i => i.TimelineItem.TargetAction == TargetAction.Create));
            Assert.AreEqual(tlchap.TimelineItem.TargetAction, TargetAction.Update);
            Assert.AreEqual(tlchap.Chapter.Name, "Hello chapter");
            Assert.IsNotNull(tlchap.Book);
            Assert.IsNull(tlchap.ContentMediaPair);

            Assert.IsNotNull(tlchap.User);
            Assert.AreEqual(tlchap.User.Id, client.AuthToken.UserId);
        }

        [TestMethod]
        public async Task Timeline_UpdateTextContent()
        {
            var client = await TestUtil.GetUserClientAsync();

            // create book
            var book = await TestUtil.Books.CreateAsync(client);

            // create chapter
            var chap = await TestUtil.Chapters.CreateAsync(client, 0, book.Id);

            // create content
            var cont = await TestUtil.TextContents.CreateAsync(client, 0, chap.Id);

            // update chapter
            cont.Content.Text = "Hello chapter content.";
            await client.Content.UpdateAsync(cont.Content);

            // get timeline
            var tl = await client.Timeline.GetLocalAsync();
            var tlconts = tl.Where(i => i.TimelineItem.TargetType == TargetType.Content)
                .Where(i => i.TimelineItem.TargetId == cont.Content.Id);
            var tlcont = tlconts.FirstOrDefault();

            Assert.IsNotNull(tlcont);
            Assert.AreEqual(tlconts.Count(), 2);
            Assert.IsTrue(tlconts.Any(i => i.TimelineItem.TargetAction == TargetAction.Create));
            Assert.AreEqual(tlcont.TimelineItem.TargetAction, TargetAction.Update);
            Assert.AreEqual(tlcont.ContentMediaPair.Content.Text, "Hello chapter content.");
            Assert.IsNotNull(tlcont.Book);
            Assert.IsNotNull(tlcont.Chapter);

            Assert.IsNotNull(tlcont.User);
            Assert.AreEqual(tlcont.User.Id, client.AuthToken.UserId);
        }

        [TestMethod]
        public async Task Timeline_DeleteBook()
        {
            var client = await TestUtil.GetUserClientAsync();

            // create and delete book
            var book = await TestUtil.Books.CreateAsync(client);
            await client.Book.DeleteAsync(book.Id);

            // get timeline
            var tl = await client.Timeline.GetLocalAsync();
            var tlbooks = tl.Where(i => i.TimelineItem.TargetType == TargetType.Book).Where(i => i.TimelineItem.TargetId == book.Id);
            var tlbook = tlbooks.FirstOrDefault();

            Assert.IsNotNull(tlbook);
            Assert.IsNull(tlbook.Book);
            Assert.AreEqual(tlbooks.Count(), 2);
            Assert.IsTrue(tlbooks.Any(i => i.TimelineItem.TargetAction == TargetAction.Create));
            Assert.AreEqual(tlbook.TimelineItem.TargetAction, TargetAction.Delete);

            Assert.IsNotNull(tlbook.User);
            Assert.AreEqual(tlbook.User.Id, client.AuthToken.UserId);
        }

        [TestMethod]
        public async Task Timeline_DeleteChapter()
        {
            var client = await TestUtil.GetUserClientAsync();

            // create book
            var book = await TestUtil.Books.CreateAsync(client);

            // create and delete chapter
            var chap = await TestUtil.Chapters.CreateAsync(client, 0, book.Id);
            await client.Chapter.DeleteAsync(chap.Id);

            // get timeline
            var tl = await client.Timeline.GetLocalAsync();
            var tlchaps = tl.Where(i => i.TimelineItem.TargetType == TargetType.Chapter).Where(i => i.TimelineItem.TargetId == chap.Id);
            var tlchap = tlchaps.FirstOrDefault();
            var tlbooks = tl.Where(i => i.TimelineItem.TargetType == TargetType.Book).Where(i => i.TimelineItem.TargetId == book.Id);

            Assert.AreEqual(tlbooks.Count(), 1);
            Assert.IsNotNull(tlchap);
            Assert.IsNull(tlchap.Chapter);
            Assert.AreEqual(tlchaps.Count(), 2);
            Assert.IsTrue(tlchaps.Any(i => i.TimelineItem.TargetAction == TargetAction.Create));
            Assert.AreEqual(tlchap.TimelineItem.TargetAction, TargetAction.Delete);

            Assert.IsNotNull(tlchap.User);
            Assert.AreEqual(tlchap.User.Id, client.AuthToken.UserId);
        }

        [TestMethod]
        public async Task Timeline_DeleteTextContent()
        {
            var client = await TestUtil.GetUserClientAsync();

            // create book
            var book = await TestUtil.Books.CreateAsync(client);

            // create chapter
            var chap = await TestUtil.Chapters.CreateAsync(client, 0, book.Id);

            // create and delete content
            var cont = await TestUtil.TextContents.CreateAsync(client, 0, chap.Id);
            await client.Content.DeleteAsync(cont.Content.Id);

            // get timeline
            var tl = await client.Timeline.GetLocalAsync();
            var tlconts = tl.Where(i => i.TimelineItem.TargetType == TargetType.Content)
                .Where(i => i.TimelineItem.TargetId == cont.Content.Id);
            var tlcont = tlconts.FirstOrDefault();
            var tlchaps = tl.Where(i => i.Chapter != null);

            Assert.AreEqual(tlchaps.Count(), 1);
            Assert.IsNotNull(tlcont);
            Assert.IsNull(tlcont.ContentMediaPair);
            Assert.AreEqual(tlconts.Count(), 2);
            Assert.IsTrue(tlconts.Any(i => i.TimelineItem.TargetAction == TargetAction.Create));
            Assert.AreEqual(tlcont.TimelineItem.TargetAction, TargetAction.Delete);

            Assert.IsNotNull(tlcont.User);
            Assert.AreEqual(tlcont.User.Id, client.AuthToken.UserId);
        }

        [TestMethod]
        public async Task TimelineStreaming_CreateBookAndChapterAndImageContent()
        {
            var client = await TestUtil.GetUserClientAsync();
            var items = new Collection<TimelineItemContainer>();
            var ex = new ExceptionContainer();

            // start streaming
            var streaming = client.Timeline.GetLocalStreaming(new SimpleReceiver
            {
                Action = (item) =>
                {
                    items.Add(item);
                },
            });

            Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(500);

                    // create book
                    var book = await TestUtil.Books.CreateAsync(client);

                    // create chapter
                    var chap = await TestUtil.Chapters.CreateAsync(client, 0, book.Id);

                    // create image content
                    var cont = await TestUtil.ImageContents.CreateAsync(client, 0, chap.Id);

                    // wait for streaming
                    await Task.Delay(10000);
                    streaming.Dispose();
                }
                catch (Exception e)
                {
                    ex.OnError(e);
                    streaming.Dispose();
                }
            });
            await streaming.ReceiveLoopAsync();
            ex.CheckError();

            Assert.AreEqual(items.Count, 3);

            Assert.IsNotNull(items[0].Book);
            Assert.AreEqual(items[0].TimelineItem.TargetAction, TargetAction.Create);
            Assert.AreEqual(items[0].TimelineItem.TargetType, TargetType.Book);

            Assert.IsNotNull(items[1].Chapter);
            Assert.AreEqual(items[1].TimelineItem.TargetAction, TargetAction.Create);
            Assert.AreEqual(items[1].TimelineItem.TargetType, TargetType.Chapter);

            Assert.IsNotNull(items[2].ContentMediaPair);
            Assert.AreEqual(items[2].TimelineItem.TargetAction, TargetAction.Create);
            Assert.AreEqual(items[2].TimelineItem.TargetType, TargetType.Content);
        }

        private class SimpleReceiver : IStreamingReceiver<TimelineItemContainer>
        {
            public Action<TimelineItemContainer> Action { get; set; }

            public async Task OnConnectionAutomaticClosedAsync()
            {
            }

            public void OnNext(TimelineItemContainer item)
            {
                this.Action?.Invoke(item);
            }
        }

        private class ExceptionContainer
        {
            private Exception e;
            public void OnError(Exception e)
            {
                this.e = e;
            }

            public void CheckError()
            {
                if (e != null)
                {
                    throw e;
                }
            }
        }
    }
}
