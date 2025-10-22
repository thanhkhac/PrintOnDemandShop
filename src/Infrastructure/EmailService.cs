using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Settings;
using Microsoft.Extensions.Options;

namespace CleanArchitectureBase.Infrastructure
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _settings;

        public EmailService(IOptions<EmailSettings> settings)
        {
            _settings = settings.Value;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            var fromAddress = new MailAddress(_settings.From, _settings.DisplayName ?? _settings.From); // 👈 thêm DisplayName
            var toAddress = new MailAddress(to);

            using var smtpClient = new SmtpClient(_settings.SmtpHost)
            {
                Port = _settings.SmtpPort,
                Credentials = new NetworkCredential(_settings.SmtpUser, _settings.SmtpPass),
                EnableSsl = _settings.EnableSsl,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false
            };

            using var mailMessage = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            await smtpClient.SendMailAsync(mailMessage);
        }
    }
}
