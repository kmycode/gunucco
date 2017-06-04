using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GunuccoSharp
{
    internal static class Utils
    {
        public static byte[] StreamToByteArray(Stream stream)
        {
            var b = new byte[stream.Length];
            stream.Seek(0, SeekOrigin.Begin);
            stream.Read(b, 0, (int)stream.Length);
            return b;
        }

        public static string StreamToBase64(Stream stream)
        {
            var bytes = StreamToByteArray(stream);
            return Convert.ToBase64String(bytes);
        }
    }
}
