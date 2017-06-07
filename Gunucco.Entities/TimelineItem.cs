using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gunucco.Entities
{
    public class TimelineItem : GunuccoEntityBase
    {
        /// <summary>
        /// The server path this timeline item came from
        /// </summary>
        [JsonProperty("server_path")]
        public string ServerPath { get; set; }

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
        public int TargetId { get; set; }

        /// <summary>
        /// Action target id. For example, when give book permission to others, this value will have others user id.
        /// </summary>
        [JsonProperty("action_target_id")]
        public int? ActionTargetId { get; set; }

        /// <summary>
        /// Target action.
        /// </summary>
        [JsonProperty("target_action")]
        internal short TargetActionValue { get; set; }

        /// <summary>
        /// Target action.
        /// </summary>
        public TargetAction TargetAction
        {
            get => (TargetAction)this.TargetActionValue;
            set => this.TargetActionValue = (short)value;
        }

        /// <summary>
        /// Actioned user id
        /// </summary>
        [JsonProperty("user_id")]
        public int UserId { get; set; }

        /// <summary>
        /// The date-time timeline item updated
        /// </summary>
        [JsonProperty("updated")]
        public DateTime Updated { get; set; }
        
        /// <summary>
        /// The integer value of listrange
        /// </summary>
        [JsonProperty("list_range")]
        internal int ListRangeValue { get; set; }

        /// <summary>
        /// The list range of the timeline item
        /// </summary>
        [JsonIgnore]
        [DBIgnore]
        public TimelineListRange ListRange
        {
            get => (TimelineListRange)this.ListRangeValue;
            set => this.ListRangeValue = (int)value;
        }
    }
}
