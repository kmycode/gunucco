using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gunucco.Entities
{
    public class AuthenticationToken
    {
        [JsonProperty("id")]
        public int UserId { get; set; }

        [JsonProperty("text_id")]
        public string UserTextId { get; set; }

        [JsonIgnore]
        public string SessionId { get; set; }

        [JsonProperty("token")]
        public string AccessToken { get; set; }

        [JsonProperty("scope")]
        internal int ScopeValue { get; set; }

        [JsonIgnore]
        public Scope Scope
        {
            get => (Scope)this.ScopeValue;
            set => this.ScopeValue = (int)value;
        }
    }
}
