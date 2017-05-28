using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Gunucco.Models.Utils
{
    public static class CryptUtil
    {
        public static string ToMD5(string plain)
        {
            using (var md5 = MD5.Create())
            {
                var result = md5.ComputeHash(Encoding.ASCII.GetBytes(plain));
                return Encoding.ASCII.GetString(result);
            }
        }

        private static readonly string passwordChars = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ@[];,./^-!#$%&'()=~`{}+*<>?_";
        public static string CreateKey(int length)
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
