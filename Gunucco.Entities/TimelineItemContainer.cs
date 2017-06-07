using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gunucco.Entities
{
    public class TimelineItemContainer
    {
        [JsonProperty("item")]
        public TimelineItem TimelineItem { get; set; }

        [JsonProperty("user")]
        public User User { get; set; }

        [JsonProperty("book", NullValueHandling = NullValueHandling.Ignore)]
        public Book Book { get; set; }

        [JsonProperty("chapter", NullValueHandling = NullValueHandling.Ignore)]
        public Chapter Chapter { get; set; }

        [JsonProperty("content_media_pair", NullValueHandling = NullValueHandling.Ignore)]
        public ContentMediaPair ContentMediaPair { get; set; }
    }
}
