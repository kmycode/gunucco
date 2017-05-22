using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gunucco.Entities
{
    public class ApiMessage
    {
        [JsonProperty("status_code")]
        public int StatusCode { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }
    }

    internal class BearerApiMessage : ApiMessage
    {
        [JsonIgnore]
        internal string WWWAuthenticateMessage { get; set; }
    }
}
