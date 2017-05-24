using Gunucco.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace GunuccoSharp.Test
{
    [TestClass]
    public class ContentTest
    {
        [TestCleanup]
        public async Task Cleanup()
        {
            await TestUtil.CleanupAsync();
        }

        [DataTestMethod]
        [DataRow("")]
        [DataRow(null)]
        [DataRow("aaaaaaaaaaaaaaaaaaaaaa")]
        public async Task CreateTextContent(string text)
        {
            var client = await TestUtil.GetUserClientAsync();

            // create book
            var book = await TestUtil.Books.CreateAsync(client);

            // create chapter
            var chap = await TestUtil.Chapters.CreateAsync(client, 0, book.Id);

            // create content
            var cont = await TestUtil.TextContents.CreateAsync(client, new ContentMediaPair
            {
                Content = new Content
                {
                    Text = text,
                }
            }, chap.Id);

            Assert.IsNotNull(cont);
            Assert.AreNotEqual(cont.Content.Id, 0);
            Assert.AreEqual(cont.Content.ChapterId, chap.Id);
            Assert.AreEqual(cont.Content.Text, text ?? string.Empty);
            Assert.AreEqual(cont.Content.Type, ContentType.Text);
            Assert.IsTrue(cont.Content.Created > DateTime.Now.AddMinutes(-3));
            Assert.IsTrue(cont.Content.LastModified > DateTime.Now.AddMinutes(-3));
        }

        [TestMethod]
        public async Task CreateTextContent_Failed_InvalidChapterId()
        {
            var client = await TestUtil.GetUserClientAsync();

            // create book
            var book = await TestUtil.Books.CreateAsync(client);

            // create chapter
            var chap = await TestUtil.Chapters.CreateAsync(client, 0, book.Id);

            // create content
            var ex = await Assert.ThrowsExceptionAsync<GunuccoErrorException>(async () =>
            {
                await TestUtil.TextContents.CreateAsync(client, 0, chap.Id + 1);
            });

            TestUtil.CheckException(ex, 404, "found", "chapter");
        }

        [TestMethod]
        public async Task CreateTextContent_Failed_OthersChapterId()
        {
            var client1 = await TestUtil.GetUserClientAsync(0);
            var client2 = await TestUtil.GetUserClientAsync(1);

            // create book
            var book1 = await TestUtil.Books.CreateAsync(client1);

            // create chapter
            var chap1 = await TestUtil.Chapters.CreateAsync(client1, 0, book1.Id);

            // create content
            var ex = await Assert.ThrowsExceptionAsync<GunuccoErrorException>(async () =>
            {
                await TestUtil.TextContents.CreateAsync(client2, 0, chap1.Id);
            });

            TestUtil.CheckException(ex, 403, "permission");
        }

        [TestMethod]
        public async Task CreateImageContent()
        {
            var client = await TestUtil.GetUserClientAsync();

            // create book
            var book = await TestUtil.Books.CreateAsync(client);

            // create chapter
            var chap = await TestUtil.Chapters.CreateAsync(client, 0, book.Id);

            // create content
            var cont = await TestUtil.ImageContents.CreateAsync(client, new ContentMediaPair
            {
                Content = new Content
                {
                    Text = "..\\..\\..\\TestFiles\\1.png",
                }
            }, chap.Id);

            Assert.IsNotNull(cont);
            Assert.AreNotEqual(cont.Content.Id, 0);
            Assert.AreEqual(cont.Content.ChapterId, chap.Id);
            Assert.AreNotEqual(cont.Media.Id, 0);
            Assert.AreEqual(cont.Content.Type, ContentType.Image);
            Assert.AreEqual(cont.Media.Extension, MediaExtension.Png);
            Assert.AreEqual(cont.Media.Type, MediaType.Image);
            Assert.IsFalse(string.IsNullOrEmpty(cont.Media.Uri));
            Assert.IsTrue(cont.Media.Uri.StartsWith("http"));
            Assert.IsTrue(cont.Content.Created > DateTime.Now.AddMinutes(-3));
            Assert.IsTrue(cont.Content.LastModified > DateTime.Now.AddMinutes(-3));
        }

        [TestMethod]
        public async Task CreateImageContent_OutsideUri()
        {
            var client = await TestUtil.GetUserClientAsync();

            // create book
            var book = await TestUtil.Books.CreateAsync(client);

            // create chapter
            var chap = await TestUtil.Chapters.CreateAsync(client, 0, book.Id);

            // create content
            var cont = await client.Content.CreateImageAsync(chap.Id, "http://kmycode.net/m.png");

            Assert.IsNotNull(cont);
            Assert.AreNotEqual(cont.Content.Id, 0);
            Assert.AreEqual(cont.Content.ChapterId, chap.Id);
            Assert.AreNotEqual(cont.Media.Id, 0);
            Assert.AreEqual(cont.Content.Type, ContentType.Image);
            Assert.AreEqual(cont.Media.Extension, MediaExtension.Outside);
            Assert.AreEqual(cont.Media.Type, MediaType.Image);
            Assert.IsFalse(string.IsNullOrEmpty(cont.Media.Uri));
            Assert.IsTrue(cont.Media.Uri.StartsWith("http"));
            Assert.AreEqual(cont.Media.Uri, "http://kmycode.net/m.png");
            Assert.IsTrue(cont.Content.Created > DateTime.Now.AddMinutes(-3));
            Assert.IsTrue(cont.Content.LastModified > DateTime.Now.AddMinutes(-3));
        }

        [DataTestMethod]
        [DataRow(PublishRange.All)]
        [DataRow(PublishRange.Private)]
        [DataRow(PublishRange.UserOnly)]
        public async Task GetTextContent_Mine(PublishRange range)
        {
            var client = await TestUtil.GetUserClientAsync();

            // create book
            var book = await TestUtil.Books.CreateAsync(client);

            // create chapter
            var chap = await TestUtil.Chapters.CreateAsync(client, 0, book.Id);

            // create content
            var cont = await TestUtil.TextContents.CreateAsync(client, 0, chap.Id);

            // get content
            var newCont = await client.Content.GetAsync(cont.Content.Id);

            Assert.IsNotNull(newCont);
            Assert.IsNotNull(newCont.Content);
            Assert.AreEqual(newCont.Content.Id, cont.Content.Id);
            Assert.AreEqual(newCont.Content.Order, cont.Content.Order);
            Assert.AreEqual(newCont.Content.Text, cont.Content.Text);
            //Assert.AreEqual(newCont.Content.Created, cont.Content.Created);
            //Assert.AreEqual(newCont.Content.LastModified, cont.Content.LastModified);
            Assert.AreEqual(newCont.Content.Type, cont.Content.Type);
        }

        [DataTestMethod]
        [DataRow(PublishRange.All, false)]
        [DataRow(PublishRange.All, true)]
        [DataRow(PublishRange.UserOnly, true)]
        public async Task GetTextContent_Others(PublishRange range, bool doLogin)
        {
            var client1 = await TestUtil.GetUserClientAsync(0);
            var client2 = await TestUtil.GetUserClientAsync(1);
            var client3 = TestUtil.GetClient();

            // create book
            var book1 = await TestUtil.Books.CreateAsync(client1);

            // create chapter
            var chap1 = await TestUtil.Chapters.CreateAsync(client1, 0, book1.Id);
            chap1.PublicRange = range;
            await client1.Chapter.UpdateAsync(chap1);

            // create content
            var cont = await TestUtil.TextContents.CreateAsync(client1, 0, chap1.Id);

            // get content
            var newCont = await (doLogin ? client2 : client3).Content.GetAsync(cont.Content.Id);

            Assert.IsNotNull(newCont);
            Assert.IsNotNull(newCont.Content);
            Assert.AreEqual(newCont.Content.Id, cont.Content.Id);
            Assert.AreEqual(newCont.Content.Order, cont.Content.Order);
            Assert.AreEqual(newCont.Content.Text, cont.Content.Text);
            //Assert.AreEqual(newCont.Content.Created, cont.Content.Created);
            //Assert.AreEqual(newCont.Content.LastModified, cont.Content.LastModified);
            Assert.AreEqual(newCont.Content.Type, cont.Content.Type);
        }

        [DataTestMethod]
        [DataRow(PublishRange.Private, true)]
        [DataRow(PublishRange.UserOnly, false)]
        public async Task GetTextContent_Failed_PermissionDenied(PublishRange range, bool doLogin)
        {
            var client1 = await TestUtil.GetUserClientAsync(0);
            var client2 = await TestUtil.GetUserClientAsync(1);
            var client3 = TestUtil.GetClient();

            // create book
            var book1 = await TestUtil.Books.CreateAsync(client1);

            // create chapter
            var chap1 = await TestUtil.Chapters.CreateAsync(client1, 0, book1.Id);

            // create content
            var cont = await TestUtil.TextContents.CreateAsync(client1, 0, chap1.Id);

            // get content
            var ex = await Assert.ThrowsExceptionAsync<GunuccoErrorException>(async () =>
            {
                await (doLogin ? client2 : client3).Content.GetAsync(cont.Content.Id);
            });

            TestUtil.CheckException(ex, 403, "permission");
        }

        [DataTestMethod]
        [DataRow(3)]
        [DataRow(1)]
        [DataRow(0)]
        public async Task GetChapterContents(int contentCount)
        {
            var client = await TestUtil.GetUserClientAsync();

            // create book
            var book = await TestUtil.Books.CreateAsync(client);

            // create chapter
            var chap = await TestUtil.Chapters.CreateAsync(client, 0, book.Id);

            // create content
            for (int i = 0; i < contentCount; i++)
            {
                await TestUtil.TextContents.CreateAsync(client, i, chap.Id);
            }

            // get contents
            var contents = await client.Chapter.GetContentsAsync(chap.Id);

            Assert.AreEqual(contents.Count(), contentCount);
        }

        [DataTestMethod]
        [DataRow(1, PublishRange.All, false)]
        [DataRow(2, PublishRange.All, true)]
        [DataRow(3, PublishRange.UserOnly, true)]
        public async Task GetChapterContents_Others(int contentCount, PublishRange range, bool doLogin)
        {
            var client1 = await TestUtil.GetUserClientAsync(0);
            var client2 = await TestUtil.GetUserClientAsync(1);
            var client3 = TestUtil.GetClient();

            // create book
            var book = await TestUtil.Books.CreateAsync(client1);

            // create chapter
            var chap = await TestUtil.Chapters.CreateAsync(client1, 0, book.Id);
            chap.PublicRange = range;
            await client1.Chapter.UpdateAsync(chap);

            // create content
            for (int i = 0; i < contentCount; i++)
            {
                await TestUtil.TextContents.CreateAsync(client1, i, chap.Id);
            }

            // get contents
            var contents = await (doLogin ? client2 : client3).Chapter.GetContentsAsync(chap.Id);

            Assert.AreEqual(contents.Count(), contentCount);
            if (contents.Count() > 2)
            {
                Assert.AreNotEqual(contents.ElementAt(0).Content.Text, contents.ElementAt(1).Content.Text);
            }
        }

        [DataTestMethod]
        [DataRow(PublishRange.Private, true)]
        [DataRow(PublishRange.UserOnly, false)]
        public async Task GetChapterContents_Failed_PermissionDenied(PublishRange range, bool doLogin)
        {
            var client1 = await TestUtil.GetUserClientAsync(0);
            var client2 = await TestUtil.GetUserClientAsync(1);
            var client3 = TestUtil.GetClient();

            // create book
            var book = await TestUtil.Books.CreateAsync(client1);

            // create chapter
            var chap = await TestUtil.Chapters.CreateAsync(client1, 0, book.Id);
            chap.PublicRange = range;
            await client1.Chapter.UpdateAsync(chap);

            // create content
            for (int i = 0; i < 3; i++)
            {
                await TestUtil.TextContents.CreateAsync(client1, i, chap.Id);
            }

            // get contents
            var ex = await Assert.ThrowsExceptionAsync<GunuccoErrorException>(async () =>
            {
                await (doLogin ? client2 : client3).Chapter.GetContentsAsync(chap.Id);
            });

            TestUtil.CheckException(ex, 403, "permission");
        }

        [TestMethod]
        public async Task DownloadImageContent()
        {
            var httpClientHandler = new HttpClientHandler()
            {
                ServerCertificateCustomValidationCallback = delegate { return true; },
            };
            var httpClient = new HttpClient(httpClientHandler);

            var client1 = await TestUtil.GetUserClientAsync(0);
            var client2 = await TestUtil.GetUserClientAsync(1);

            // create book
            var book1 = await TestUtil.Books.CreateAsync(client1);

            // create chapter
            var chap1 = await TestUtil.Chapters.CreateAsync(client1, 0, book1.Id);
            chap1.PublicRange = PublishRange.All;
            await client1.Chapter.UpdateAsync(chap1);

            // create content
            var cont1 = await TestUtil.ImageContents.CreateAsync(client1, 0, chap1.Id);

            // download image
            var response = await httpClient.GetAsync(cont1.Media.Uri);
            var downloadedData = await response.Content.ReadAsByteArrayAsync();

            // get original image
            var originalData = File.ReadAllBytes("..\\..\\..\\TestFiles\\1.png");

            Assert.AreEqual(response.Content.Headers.ContentType.MediaType, "image/png");
            Assert.IsTrue(downloadedData.SequenceEqual(originalData));
        }

        [TestMethod]
        public async Task DownloadImageContent_Failed_PermisionDenied()
        {
            var httpClientHandler = new HttpClientHandler()
            {
                ServerCertificateCustomValidationCallback = delegate { return true; },
            };
            var httpClient = new HttpClient(httpClientHandler);

            var client1 = await TestUtil.GetUserClientAsync(0);
            var client2 = await TestUtil.GetUserClientAsync(1);

            // create book
            var book1 = await TestUtil.Books.CreateAsync(client1);

            // create chapter
            var chap1 = await TestUtil.Chapters.CreateAsync(client1, 0, book1.Id);

            // create content
            var cont1 = await TestUtil.ImageContents.CreateAsync(client1, 0, chap1.Id);

            // download image
            var response = await httpClient.GetAsync(cont1.Media.Uri);
            await response.Content.ReadAsByteArrayAsync();

            var json = await response.Content.ReadAsStringAsync();
            var err = JsonConvert.DeserializeObject<ApiMessage>(json);

            TestUtil.CheckException(err, 403, "permission");
        }

        [TestMethod]
        public async Task UpdateTextContent()
        {
            var client = await TestUtil.GetUserClientAsync();

            // create book
            var book = await TestUtil.Books.CreateAsync(client);

            // create chapter
            var chap = await TestUtil.Chapters.CreateAsync(client, 0, book.Id);

            // create content
            var cont = await TestUtil.TextContents.CreateAsync(client, 0, chap.Id);
            cont.Content.Text = "Real watch";
            var mes = await client.Content.UpdateAsync(cont.Content);

            // get content
            var newCont = await client.Content.GetAsync(cont.Content.Id);

            Assert.AreEqual(mes.StatusCode, 200);
            Assert.IsTrue(mes.Message.Contains("succeed"));
            Assert.AreEqual(newCont.Content.Text, "Real watch");
            Assert.AreEqual(newCont.Content.Id, cont.Content.Id);
        }

        [TestMethod]
        public async Task UpdateTextContent_WithChangeChapter()
        {
            var client = await TestUtil.GetUserClientAsync();

            // create book
            var book = await TestUtil.Books.CreateAsync(client);

            // create chapter
            var chap1 = await TestUtil.Chapters.CreateAsync(client, 0, book.Id);
            var chap2 = await TestUtil.Chapters.CreateAsync(client, 1, book.Id);

            // create content
            var cont = await TestUtil.TextContents.CreateAsync(client, 0, chap1.Id);
            cont.Content.ChapterId = chap2.Id;
            var mes = await client.Content.UpdateAsync(cont.Content);

            // get content
            var newCont = await client.Content.GetAsync(cont.Content.Id);

            Assert.AreEqual(newCont.Content.ChapterId, chap2.Id);
            Assert.AreEqual((await client.Chapter.GetContentsAsync(chap1.Id)).Count(), 0);
            Assert.AreEqual((await client.Chapter.GetContentsAsync(chap2.Id)).Count(), 1);
        }

        [TestMethod]
        public async Task UpdateTextContent_WithChangeChapter_ChangeBook()
        {
            var client = await TestUtil.GetUserClientAsync();

            // create book
            var book1 = await TestUtil.Books.CreateAsync(client, 0);
            var book2 = await TestUtil.Books.CreateAsync(client, 1);

            // create chapter
            var chap1 = await TestUtil.Chapters.CreateAsync(client, 0, book1.Id);
            var chap2 = await TestUtil.Chapters.CreateAsync(client, 1, book2.Id);

            // create content
            var cont = await TestUtil.TextContents.CreateAsync(client, 0, chap1.Id);
            cont.Content.ChapterId = chap2.Id;
            var mes = await client.Content.UpdateAsync(cont.Content);

            // get content
            var newCont = await client.Content.GetAsync(cont.Content.Id);

            Assert.AreEqual(newCont.Content.ChapterId, chap2.Id);
            Assert.AreEqual((await client.Chapter.GetContentsAsync(chap1.Id)).Count(), 0);
            Assert.AreEqual((await client.Chapter.GetContentsAsync(chap2.Id)).Count(), 1);
        }

        [TestMethod]
        public async Task UpdateTextContent_Failed_OthersChapter()
        {
            var client1 = await TestUtil.GetUserClientAsync(0);
            var client2 = await TestUtil.GetUserClientAsync(1);

            // create book
            var book1 = await TestUtil.Books.CreateAsync(client1);

            // create chapter
            var chap1 = await TestUtil.Chapters.CreateAsync(client1, 0, book1.Id);
            chap1.PublicRange = PublishRange.All;
            await client1.Chapter.UpdateAsync(chap1);

            // create content
            var cont1 = await TestUtil.TextContents.CreateAsync(client1, 0, chap1.Id);

            // update content
            var ex = await Assert.ThrowsExceptionAsync<GunuccoErrorException>(async () =>
            {
                await client2.Content.UpdateAsync(cont1.Content);
            });

            TestUtil.CheckException(ex, 403, "permission");
        }

        [TestMethod]
        public async Task UpdateTextContent_WithChangeOrder()
        {
            var client = await TestUtil.GetUserClientAsync();

            // create book
            var book = await TestUtil.Books.CreateAsync(client);

            // create chapter
            var chap = await TestUtil.Chapters.CreateAsync(client, 0, book.Id);

            // create content
            var cont1 = await TestUtil.TextContents.CreateAsync(client, 0, chap.Id);
            cont1.Content.Order = 5;
            await client.Content.UpdateAsync(cont1.Content);
            var cont2 = await TestUtil.TextContents.CreateAsync(client, 0, chap.Id);
            cont2.Content.Order = 2;
            await client.Content.UpdateAsync(cont2.Content);

            // get contents
            var contents = await client.Chapter.GetContentsAsync(chap.Id);

            Assert.AreEqual(contents.Count(), 2);
            Assert.AreEqual(contents.First().Content.Order, 2);
            Assert.AreEqual(contents.First().Content.Id, cont2.Content.Id);
            Assert.AreEqual(contents.ElementAt(1).Content.Order, 5);
            Assert.AreEqual(contents.ElementAt(1).Content.Id, cont1.Content.Id);
        }

        [TestMethod]
        public async Task DeleteTextContent()
        {
            var client = await TestUtil.GetUserClientAsync();

            // create book
            var book = await TestUtil.Books.CreateAsync(client);

            // create chapter
            var chap = await TestUtil.Chapters.CreateAsync(client, 0, book.Id);

            // create content
            var cont = await TestUtil.TextContents.CreateAsync(client, 0, chap.Id);

            // delete content
            var mes = await client.Content.DeleteAsync(cont.Content.Id);
            TestUtil.TextContents.Remove(cont);

            // get content
            var ex = await Assert.ThrowsExceptionAsync<GunuccoErrorException>(async () =>
            {
                await client.Content.GetAsync(cont.Content.Id);
            });

            TestUtil.CheckException(mes, 200, "succeed");
            TestUtil.CheckException(ex, 404, "found");
        }

        [TestMethod]
        public async Task DeleteImageContent()
        {
            var httpClientHandler = new HttpClientHandler()
            {
                ServerCertificateCustomValidationCallback = delegate { return true; },
            };
            var httpClient = new HttpClient(httpClientHandler);
            var client = await TestUtil.GetUserClientAsync();

            // create book
            var book = await TestUtil.Books.CreateAsync(client);

            // create chapter
            var chap = await TestUtil.Chapters.CreateAsync(client, 0, book.Id);

            // create content
            var cont = await TestUtil.ImageContents.CreateAsync(client, 0, chap.Id);

            // delete content
            var mes = await client.Content.DeleteAsync(cont.Content.Id);
            TestUtil.ImageContents.Remove(cont);

            // get content
            var ex = await Assert.ThrowsExceptionAsync<GunuccoErrorException>(async () =>
            {
                await client.Content.GetAsync(cont.Content.Id);
            });

            var response = await httpClient.GetAsync(cont.Media.Uri);

            TestUtil.CheckException(mes, 200, "succeed");
            Assert.AreEqual((int)response.StatusCode, 404);
        }

        [TestMethod]
        public async Task DeleteTextContent_Failed_InvalidId()
        {
            var client = await TestUtil.GetUserClientAsync();

            // create book
            var book = await TestUtil.Books.CreateAsync(client);

            // create chapter
            var chap = await TestUtil.Chapters.CreateAsync(client, 0, book.Id);

            // create content
            var cont = await TestUtil.TextContents.CreateAsync(client, 0, chap.Id);

            // delete content
            var ex = await Assert.ThrowsExceptionAsync<GunuccoErrorException>(async () =>
            {
                await client.Content.DeleteAsync(cont.Content.Id + 1);
            });

            TestUtil.CheckException(ex, 404, "found");
        }

        [TestMethod]
        public async Task DeleteTextContent_Failed_OthersContent()
        {
            var client1 = await TestUtil.GetUserClientAsync(0);
            var client2 = await TestUtil.GetUserClientAsync(1);

            // create book
            var book = await TestUtil.Books.CreateAsync(client1);

            // create chapter
            var chap = await TestUtil.Chapters.CreateAsync(client1, 0, book.Id);

            // create content
            var cont = await TestUtil.TextContents.CreateAsync(client1, 0, chap.Id);

            // delete content
            var ex = await Assert.ThrowsExceptionAsync<GunuccoErrorException>(async () =>
            {
                await client2.Content.DeleteAsync(cont.Content.Id);
            });

            TestUtil.CheckException(ex, 403, "permission");
        }
    }
}
