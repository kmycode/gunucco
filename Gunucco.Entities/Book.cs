using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gunucco.Entities
{
    public class Book : GunuccoEntityBase
    {
        /// <summary>
        /// The date and time book created
        /// </summary>
        [JsonProperty("created")]
        public DateTime Created { get; set; }

        /// <summary>
        /// Book name
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }
    }
}
