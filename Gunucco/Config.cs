using MailKit.Security;
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
        /// Gunucco version to grasp api or function set server provides.
        /// </summary>
        public static string ServerVersion { get; set; }

        /// <summary>
        /// Server administrator name.
        /// </summary>
        public static string AdministratorName { get; set; }

        /// <summary>
        /// Server administrator HP Uri.
        /// </summary>
        public static string AdministratorUri { get; set; }

        /// <summary>
        /// Server path starts with 'http://' or 'https://' and not ends with '/'
        /// </summary>
        public static string ServerPath { get; set; }

        /// <summary>
        /// Server is in debug mode or not.
        /// </summary>
        public static bool IsDebugMode { get; set; }

        /// <summary>
        /// To allow post files which media source is outside (getting media file from outside servers)
        /// </summary>
        public static bool IsAllowOutsideMedias { get; set; }

        /// <summary>
        /// To allow new user signing up.
        /// </summary>
        public static bool IsAllowNewSignUp { get; set; }

        #region Mail Setting

        /// <summary>
        /// Email validation need or not. If turn false, you must do something to block spams.
        /// </summary>
        public static bool IsEmailValidationNeed { get; set; }

        /// <summary>
        /// Smtp server for send validation mail.
        /// </summary>
        public static string SmtpServer { get; set; }

        /// <summary>
        /// Smtp port number for send validation mail.
        /// </summary>
        public static int SmtpPort { get; set; }

        /// <summary>
        /// Smtp socket options for send validation mail.
        /// </summary>
        public static SecureSocketOptions SmtpSecureSocketOptions { get; set; }

        /// <summary>
        /// Smtp account id for connecting server.
        /// </summary>
        public static string SmtpAccountId { get; set; }

        /// <summary>
        /// Smtp password for connecting server.
        /// </summary>
        public static string SmtpPassword { get; set; }

        /// <summary>
        /// Mail from address.
        /// </summary>
        public static string MailFrom { get; set; }

        /// <summary>
        /// Mail from name.
        /// </summary>
        public static string MailFromName { get; set; }

        #endregion
    }
}
