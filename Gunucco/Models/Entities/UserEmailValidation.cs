using Gunucco.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gunucco.Models.Entities
{
    public class UserEmailValidation : GunuccoEntityBase
    {
        public int UserId { get; set; }

        public string ValidateKey { get; set; }
    }
}
