using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gunucco.Entities
{
    public class Chapter : GunuccoEntityBase
    {
        /// <summary>
        /// Parent chapter id. Null if no parents.
        /// </summary>
        [JsonProperty("parent_id")]
        public int? ParentId { get; set; }

        /// <summary>
        /// Book id.
        /// </summary>
        [JsonProperty("book_id")]
        public int BookId { get; set; }

        /// <summary>
        /// Chapter name
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Chapter order
        /// </summary>
        [JsonProperty("order")]
        public int Order { get; set; }

        /// <summary>
        /// Chapter publish range
        /// </summary>
        [JsonProperty("public_range")]
        internal short PublicRangeValue { get; set; }

        /// <summary>
        /// Chapter publish range
        /// </summary>
        [JsonIgnore]
        [DBIgnore]
        public PublishRange PublicRange
        {
            get => (PublishRange)this.PublicRangeValue;
            set => this.PublicRangeValue = (short)value;
        }
    }
}
