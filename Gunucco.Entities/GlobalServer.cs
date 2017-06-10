using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gunucco.Entities
{
    public class GlobalServer : GunuccoEntityBase
    {
        /// <summary>
        /// Server path
        /// </summary>
        [JsonProperty("server_path")]
        public string ServerPath { get; set; }

        /// <summary>
        /// Which blocked or not by self server
        /// </summary>
        [JsonProperty("is_blocking")]
        public bool IsBlocking { get; set; }
    }
}
