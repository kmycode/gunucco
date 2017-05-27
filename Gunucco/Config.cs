using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gunucco
{
    /// <summary>
    /// Server config. Usually, values are saved in appsettings.json.
    /// See: https://github.com/kmycode/gunucco/wiki/Quick-Start-for-Server-Administrators
    /// </summary>
    public static class Config
    {
        /// <summary>
        /// Server path starts with 'http://' or 'https://' and not ends with '/'
        /// </summary>
        public static string ServerPath { get; set; }

        /// <summary>
        /// Server is in debug mode or not.
        /// </summary>
        public static bool IsDebugMode { get; set; }
    }
}
