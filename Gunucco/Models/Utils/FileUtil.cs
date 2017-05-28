using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Gunucco.Models.Utils
{
    public static class FileUtil
    {
        public static string ToBase64(this IFormFile file)
        {
            var stream = file.OpenReadStream();
            var buf = new byte[stream.Length];
            stream.Seek(0, SeekOrigin.Begin);
            stream.Read(buf, 0, (int)stream.Length);
            var base64 = Convert.ToBase64String(buf);
            return base64;
        }
    }
}
