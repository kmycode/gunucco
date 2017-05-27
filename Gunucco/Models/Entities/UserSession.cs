using Gunucco.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gunucco.Models.Entities
{
    public class UserSession
    {
        public string Id { get; set; }

        public int UserId { get; set; }

        public string UserTextId { get; set; }

        public DateTime ExpireDateTime { get; set; }

        internal int ScopeValue { get; set; }

        [DBIgnore]
        public Scope Scope
        {
            get => (Scope)this.ScopeValue;
            set => this.ScopeValue = (int)value;
        }

        public static UserSession Create(User user, Scope scope)
        {
            return new UserSession
            {
                Id = CreateKey(64),
                UserId = user.Id,
                UserTextId = user.TextId,
                ExpireDateTime = DateTime.Now.AddHours(24),
                Scope = scope,
            };
        }

        private static readonly string passwordChars = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ@[];,./^-!#$%&'()=~`{}+*<>?_";
        private static string CreateKey(int length)
        {
            var sb = new StringBuilder(length);
            var r = new Random();

            for (int i = 0; i < length; i++)
            {
                int pos = r.Next(passwordChars.Length);
                char c = passwordChars[pos];
                sb.Append(c);
            }

            return sb.ToString();
        }
    }
}
