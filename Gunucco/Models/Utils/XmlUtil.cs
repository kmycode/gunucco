using Gunucco.Entities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace Gunucco.Models.Utils
{
    public class XmlUtil
    {
        public static string ParseXml(IEnumerable<TimelineItemContainer> containers)
        {
            var timezone = DateTime.Now.ToString("zzz").Replace(":", "");
            string dateFormat = "ddd, d MMM yyyy HH:mm:ss " + timezone;
            var culture = new CultureInfo("en-US");
            var html = HtmlEncoder.Create(new TextEncoderSettings());

            var sb = new StringBuilder();

            // rss header
            sb.Append("<?xml version=\"1.0\" encoding=\"utf-8\"?>\n");
            sb.Append("<rss version=\"2.0\" xmlns:rss=\"http://purl.org/rss/1.0/\" xmlns:dc=\"http://purl.org/dc/elements/1.1/\" xmlns:content=\"http://purl.org/rss/1.0/modules/content/\" xmlns:atom=\"http://www.w3.org/2005/Atom\">\n");

            // start channels
            sb.Append("<channel>\n");

            // set channels properties
            sb.Append($"<title>RSS of Gunucco {Config.ServerPath} ({Config.ServerVersion})</title>\n");
            sb.Append($"<link>{Config.ServerPath}/</link>\n");
            sb.Append("<description>Gunucco news</description>\n");
            sb.Append($"<language>{Config.ServerLanguage}</language>\n");
            sb.Append($"<atom:link rel=\"self\" href=\"{Config.ServerPath}/api/v1/rss/localtimeline\" type=\"application/rss+xml\" />\n");
            sb.Append($"<pubDate>{DateTime.Now.ToString(dateFormat, culture)}</pubDate>\n");
            if (containers.Any())
            {
                sb.Append($"<lastBuildDate>{containers.First().TimelineItem.Updated.ToString(dateFormat, culture)}</lastBuildDate>\n");
            }
            sb.Append("<docs>http://blogs.law.harvard.edu/tech/rss</docs>\n");

            // set items
            int lastChapterId = 0;
            int count = 0;
            foreach (var item in containers)
            {
                if (item.TimelineItem.TargetType == TargetType.Book && item.Book != null)
                {
                    sb.Append("<item>\n");
                    sb.Append($"<title>{item.Book.Name}</title>\n");
                    sb.Append($"<link>{Config.ServerPath}/web/book/{item.Book.Id}</link>\n");
                    sb.Append($"<description>{html.Encode(item.Book.Description)}</description>\n");
                    sb.Append($"<pubDate>{item.TimelineItem.Updated.ToString(dateFormat, culture)}</pubDate>\n");
                    sb.Append($"<guid>{Config.ServerPath}/web/book/{item.Book.Id}</guid>\n");
                    sb.Append("</item>\n");

                    if (count++ >= 20) break;
                }
                else if (item.TimelineItem.TargetType == TargetType.Chapter && item.Chapter != null)
                {
                    if (item.Chapter.Id != lastChapterId)
                    {
                        sb.Append("<item>\n");
                        sb.Append($"<title>{item.Chapter.Name}</title>\n");
                        sb.Append($"<link>{Config.ServerPath}/web/book/{item.Chapter.BookId}/chapter/{item.Chapter.Id}</link>\n");
                        sb.Append($"<description>{html.Encode(item.Chapter.Name)}</description>\n");
                        sb.Append($"<pubDate>{item.TimelineItem.Updated.ToString(dateFormat, culture)}</pubDate>\n");
                        sb.Append($"<guid>{Config.ServerPath}/web/book/{item.Book.Id}</guid>\n");
                        sb.Append("</item>\n");
                        lastChapterId = item.Chapter.Id;

                        if (count++ >= 20) break;
                    }
                }
                else if (item.TimelineItem.TargetType == TargetType.Content && item.ContentMediaPair?.Content != null && item.Chapter != null)
                {
                    if (item.Chapter.Id != lastChapterId)
                    {
                        sb.Append("<item>\n");
                        sb.Append($"<title>{item.Chapter.Name}</title>\n");
                        sb.Append($"<link>{Config.ServerPath}/web/book/{item.Chapter.BookId}/chapter/{item.Chapter.Id}</link>\n");
                        sb.Append($"<description>{html.Encode(item.ContentMediaPair.Content.Text)}</description>\n");
                        sb.Append($"<content:encoded><![CDATA[{html.Encode(item.ContentMediaPair.Content.Text)}]]></description>\n");
                        sb.Append($"<pubDate>{item.TimelineItem.Updated.ToString(dateFormat)}</pubDate>\n");
                        sb.Append($"<guid isPermaLink=\"false\">content-{item.ContentMediaPair.Content.Id}</guid>\n");
                        sb.Append("</item>\n");
                        lastChapterId = item.Chapter.Id;

                        if (count++ >= 20) break;
                    }
                }
            }

            // end channels
            sb.Append("</channel>\n");

            // rss footer
            sb.Append("</rss>\n");

            return sb.ToString();
        }
    }
}
