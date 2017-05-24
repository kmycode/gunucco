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
        /// Uploading media data with base64 encoded
        /// (Upload api only, not save in database)
        /// </summary>
        [DBIgnore]
        [JsonProperty("media_data")]
        internal string MediaData { get; set; }

        /// <summary>
        /// Media data row (Converted media data to byte array)
        /// </summary>
        [DBIgnore]
        [JsonIgnore]
        internal byte[] MediaDataRow { get; set; }

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
        /// 16bit integer of media extension
        /// </summary>
        [JsonProperty("extension")]
        internal short ExtensionValue { get; set; }

        /// <summary>
        /// Media extension. If source is outside, this property is ignored.
        /// </summary>
        [JsonIgnore]
        [DBIgnore]
        public MediaExtension Extension
        {
            get => (MediaExtension)this.ExtensionValue;
            set => this.ExtensionValue = (short)value;
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

    public enum MediaExtension : short
    {
        Outside = 100,      // If MediaSource is outside
        Png = 101,
        Jpeg = 102,
        Gif = 103,
    }
}
