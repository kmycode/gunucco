using Gunucco.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gunucco.Entities
{
    public class UserSessionData
    {
        [JsonProperty("id_hash")]
        public string IdHash { get; set; }

        [JsonProperty("expire")]
        public DateTime Expire { get; set; }

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
