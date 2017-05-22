using Gunucco.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gunucco.Common
{
    public class GunuccoException : Exception
    {
        public ApiMessage Error { get; }

        public GunuccoException(ApiMessage error) : base() { this.Error = error; }

        public GunuccoException(string message, ApiMessage error) : base(message) { this.Error = error; }
    }
}
