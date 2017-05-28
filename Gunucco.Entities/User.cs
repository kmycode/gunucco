using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Gunucco.Entities
{
    public class User : GunuccoEntityBase
    {
        /// <summary>
        /// User text id (ex. kmys)
        /// </summary>
        [JsonProperty("text_id")]
        public string TextId { get; set; }

        /// <summary>
        /// User password hash string (MD5)
        /// </summary>
        [JsonIgnore]
        public string PasswordHash { get; set; }

        /// <summary>
        /// User display name
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// User description string
        /// </summary>
        [JsonProperty("description")]
        public string Description { get; set; }

        /// <summary>
        /// Is this user anonymous. Usually false, but rarely true is set.
        /// </summary>
        [DBIgnore]
        [JsonProperty("is_anonymous")]
        public bool IsAnonymous { get; set; }

        internal bool IsMatchPassword(string password)
        {
            using (var md5 = MD5.Create())
            {
                var result = md5.ComputeHash(Encoding.ASCII.GetBytes(password));
                var hashStr = Encoding.ASCII.GetString(result);
                return hashStr == this.PasswordHash;
            }
        }

        internal void SetPassword(string password)
        {
            using (var md5 = MD5.Create())
            {
                var result = md5.ComputeHash(Encoding.ASCII.GetBytes(password));
                this.PasswordHash = Encoding.ASCII.GetString(result);
            }
        }
    }
}
