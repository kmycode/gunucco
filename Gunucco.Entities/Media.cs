using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gunucco.Entities
{
    public class Media : GunuccoEntityBase
    {
        /// <summary>
        /// Content id
        /// </summary>
        [JsonProperty("content_id")]
        public int ContentId { get; set; }

        /// <summary>
        /// 16bit integer of media type
        /// </summary>
        [JsonProperty("type")]
        internal short TypeValue { get; set; }

        /// <summary>
        /// Media type
        /// </summary>
        [JsonIgnore]
        [DBIgnore]
        public MediaType Type
        {
            get => (MediaType)this.TypeValue;
            set => this.TypeValue = (short)value;
        }

        /// <summary>
        /// 16bit integer of media source
        /// </summary>
        [JsonProperty("media_source")]
        internal short SourceValue { get; set; }

        /// <summary>
        /// Media source
        /// </summary>
        [JsonIgnore]
        [DBIgnore]
        public MediaSource Source
        {
            get => (MediaSource)this.SourceValue;
            set => this.SourceValue = (short)value;
        }

        /// <summary>
        /// Media file path. Only server can read and write because of security.
        /// If media source is self, this is relative path ef: /image.png
        /// If media source is outside, this starts with http:// or https://
        /// </summary>
        [JsonIgnore]
        internal string FilePath { get; set; }

        /// <summary>
        /// Media uri to download
        /// </summary>
        [DBIgnore]
        [JsonProperty("uri")]
        public string Uri { get; set; }
    }

    public enum MediaType : short
    {
        Image = 101,
    }
}
