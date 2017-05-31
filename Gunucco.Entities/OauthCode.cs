using Gunucco.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gunucco.Entities
{
    public class OauthCode : GunuccoEntityBase
    {
        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonIgnore]
        internal string SessionId { get; set; }

        [JsonProperty("user_id")]
        internal int UserId { get; set; }

        [JsonProperty("user_text_id")]
        internal string UserTextId { get; set; }

        [JsonProperty("scope")]
        internal short ScopeValue { get; set; }

        [JsonIgnore]
        [DBIgnore]
        public Scope Scope
        {
            get => (Scope)this.ScopeValue;
            set => this.ScopeValue = (short)value;
        }

        [JsonProperty("expire_date_time")]
        public DateTime ExpireDateTime { get; set; }

        [DBIgnore]
        [JsonProperty("oauth_uri")]
        public string OauthUri { get; set; }
    }
}
