using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gunucco.Entities
{
    public class BookPermission : GunuccoEntityBase
    {
        /// <summary>
        /// Target user id
        /// </summary>
        [JsonProperty("user_id")]
        public int UserId { get; set; }

        /// <summary>
        /// Target id type
        /// </summary>
        [JsonProperty("target_type")]
        internal short TargetTypeValue { get; set; }

        /// <summary>
        /// Target id type. If this value is '101', TargetId becomes BookId.
        /// </summary>
        [JsonIgnore]
        [DBIgnore]
        public TargetType TargetType
        {
            get => (TargetType)this.TargetTypeValue;
            set => this.TargetTypeValue = (short)value;
        }

        /// <summary>
        /// Target id.
        /// </summary>
        [JsonProperty("target_id")]
        public int? TargetId { get; set; }
    }

    public enum TargetType : short
    {
        Book = 101,
        Chapter = 102,
        Content = 103,
    }
}
