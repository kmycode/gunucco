using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gunucco.Entities
{
    public class Content : GunuccoEntityBase
    {
        /// <summary>
        /// The chapter content belongs to
        /// </summary>
        [JsonProperty("chapter")]
        public int ChapterId { get; set; }

        /// <summary>
        /// 16bit integer value of content type
        /// </summary>
        [JsonProperty("content_type")]
        internal short TypeValue { get; set; }

        /// <summary>
        /// Content type
        /// </summary>
        [JsonIgnore]
        [DBIgnore]
        public ContentType Type
        {
            get => (ContentType)this.TypeValue;
            set => this.TypeValue = (short)value;
        }

        /// <summary>
        /// Order of each contents in chapter.
        /// Starts with 0. Order with content id if same order number.
        /// </summary>
        [JsonProperty("order")]
        public int Order { get; set; }

        /// <summary>
        /// Content text for composition content
        /// </summary>
        [JsonProperty("text")]
        public string Text { get; set; }

        /// <summary>
        /// Content to show HTML format
        /// </summary>
        [JsonIgnore]
        [DBIgnore]
        public string HtmlText => this.Text.Replace("\n", "<br />");

        /// <summary>
        /// Created date time
        /// </summary>
        [JsonProperty("created")]
        public DateTime Created { get; set; }

        /// <summary>
        /// Last modified date time
        /// </summary>
        [JsonProperty("last_modified")]
        public DateTime LastModified { get; set; }
    }

    public enum ContentType : short
    {
        Text = 101,
        Image = 102,
    }
}
