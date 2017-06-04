using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gunucco.Entities
{
    public class Book : GunuccoEntityBase
    {
        /// <summary>
        /// Book name
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Book order
        /// </summary>
        [JsonProperty("order")]
        public int Order { get; set; }

        /// <summary>
        /// The description of the book
        /// </summary>
        [JsonProperty("description")]
        public string Description { get; set; }

        /// <summary>
        /// The range of posting this book updated information
        /// </summary>
        [JsonProperty("post_to")]
        internal short PostToValue { get; set; }

        /// <summary>
        /// The range of posting this book
        /// </summary>
        [JsonIgnore]
        [DBIgnore]
        public PostTo PostTo
        {
            get => (PostTo)this.PostToValue;
            set => this.PostToValue = (short)value;
        }

        /// <summary>
        /// The date and time book created
        /// </summary>
        [JsonProperty("created")]
        public DateTime Created { get; set; }

        /// <summary>
        /// The date and time book modified
        /// </summary>
        [JsonProperty("last_modified")]
        public DateTime LastModified { get; set; }
    }
}
