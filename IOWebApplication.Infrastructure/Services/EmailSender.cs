using IOWebApplication.Infrastructure.Contracts;
using Microsoft.Extensions.Configuration;
using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace IOWebApplication.Infrastructure.Services
{
    // This class is used by the application to send email for account confirmation and password reset.
    // For more details see https://go.microsoft.com/fwlink/?LinkID=532713
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration configuration;

        public EmailSender(IConfiguration _configuration)
        {
            configuration = _configuration;
        }

        public Task SendMail(string address, string subject, string body)
        {
            return SendMail(new string[] { address }, subject, body);
        }

        public Task SendMail(string[] addressList, string subject, string body)
        {
            string server = configuration.GetSection("EmailService").GetValue<string>("server");
            SmtpClient client = new SmtpClient(server);
            var emailUser = configuration.GetSection("EmailService").GetValue<string>("user");
            var emailPass = configuration.GetSection("EmailService").GetValue<string>("pass");
            if (!string.IsNullOrEmpty(emailUser))
            {
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential(emailUser, emailPass);
            }
            else
            {
                client.UseDefaultCredentials = true;
            };

            MailMessage mailMessage = new MailMessage();
            string mailbox = configuration.GetSection("EmailService").GetValue<string>("mailbox");
            mailMessage.From = new MailAddress(mailbox);
            foreach (var address in addressList)
            {
                mailMessage.Bcc.Add(address);
            }
            mailMessage.Body = body;
            mailMessage.IsBodyHtml = true;
            mailMessage.Subject = subject;
            try
            {
                return client.SendMailAsync(mailMessage);
            }
            catch (Exception ex)
            {
                return Task.FromException(ex);
            }
        }
    }
}
