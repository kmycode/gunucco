using Gunucco.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace GunuccoSharp
{
    public class GunuccoException : Exception
    {
        public GunuccoException() : base() { }

        public GunuccoException(string message) : base(message) { }

        public GunuccoException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class GunuccoErrorException : Exception
    {
        public ApiMessage Error { get; }

        public int StatusCode { get; set; }

        public GunuccoErrorException(ApiMessage error) : base() { this.Error = error; }

        public GunuccoErrorException(string message, ApiMessage error, int code) : base(message) { this.Error = error; this.StatusCode = code; }
    }
}
