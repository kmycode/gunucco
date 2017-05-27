using Gunucco.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gunucco.Models.Entities
{
    public class AuthorizationData
    {
        public AuthorizationToken AuthToken { get; set; }

        public UserSession Session { get; set; }

        public User User { get; set; }

        public bool IsAuthed => this.User != null &&
            this.User.Id != 0 &&
            !string.IsNullOrEmpty(this.AuthToken.SessionId) &&
            !string.IsNullOrEmpty(this.Session.Id);
    }
}
