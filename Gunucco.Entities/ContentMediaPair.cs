using Gunucco.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gunucco.Entities
{
    public class ContentMediaPair
    {
        [JsonProperty("content")]
        public Content Content { get; set; }

        [JsonProperty("media", NullValueHandling = NullValueHandling.Ignore)]
        public Media Media { get; set; }
    }
}
