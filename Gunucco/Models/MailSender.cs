using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gunucco.Models
{
    public class MailSender
    {
        private static Logger log = LogManager.GetCurrentClassLogger();

        public string SmtpServer { get; set; }

        public int SmtpPort { get; set; }

        public SecureSocketOptions SecureSocketOptions { get; set; }

        public string SmtpAccountId { get; set; }

        public string SmtpPassword { get; set; }

        public string From { get; set; }

        public string FromName { get; set; }

        public string To { get; set; }

        public string Subject { get; set; }

        public string Text { get; set; }

        public void Send()
        {
            Task.Run(async () => await this.SendAsync()).Wait();
        }

        public async Task SendAsync()
        {
            try
            {
                var emailMessage = new MimeMessage();

                emailMessage.From.Add(new MailboxAddress(this.FromName, this.From));
                emailMessage.To.Add(new MailboxAddress("", this.To));
                emailMessage.Subject = this.Subject;
                emailMessage.Body = new TextPart("plain") { Text = this.Text, };

                using (var client = new SmtpClient())
                {
                    await client.ConnectAsync(this.SmtpServer, this.SmtpPort, this.SecureSocketOptions);
                    await client.AuthenticateAsync(this.SmtpAccountId, this.SmtpPassword);
                    await client.SendAsync(emailMessage);
                    await client.DisconnectAsync(true);
                }
            }
            catch (Exception ex)
            {
                log.Error(ex, "Send email failed.");
                throw ex;
            }
        }

        public static MailSender Create()
        {
            var sender = new MailSender
            {
                SmtpServer = Config.SmtpServer,
                SmtpPort = Config.SmtpPort,
                SecureSocketOptions = Config.SmtpSecureSocketOptions,
                SmtpAccountId = Config.SmtpAccountId,
                SmtpPassword = Config.SmtpPassword,
                From = Config.MailFrom,
                FromName = Config.MailFromName,
            };
            return sender;
        }
    }
}
