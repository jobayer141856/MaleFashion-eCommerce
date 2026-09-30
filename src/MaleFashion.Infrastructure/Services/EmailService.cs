using MaleFashion.Application.Contracts.Services;
using Microsoft.Extensions.Options;
using MimeKit;
using System;
using System.Text;
using MailKit.Net.Smtp;
using MaleFashion.Domain.Utilities;


namespace MaleFashion.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly SmtpSettings _smtpSettings;

        public EmailService(IOptions<SmtpSettings> smtpSettings)
        {
            _smtpSettings = smtpSettings.Value;
        }

        public async Task SendEmailAsync(string receiverName, string reciverEmail, string subject, string body)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_smtpSettings.FromName, _smtpSettings.FromEmail));
            message.To.Add(new MailboxAddress(receiverName, reciverEmail));
            message.Subject = subject;

            message.Body = new TextPart("html")
            {
                Text = body
            };

            using (var client = new SmtpClient())
            {
                var socketOption = _smtpSettings.SmtpEncryption switch
                {
                    SmtpEncryptionTypes.SslTls => MailKit.Security.SecureSocketOptions.SslOnConnect,
                    SmtpEncryptionTypes.StartTls => MailKit.Security.SecureSocketOptions.StartTls,
                    SmtpEncryptionTypes.Normal => MailKit.Security.SecureSocketOptions.None,
                    _ => MailKit.Security.SecureSocketOptions.Auto
                };

                await client.ConnectAsync(_smtpSettings.Host, _smtpSettings.Port, socketOption);

                client.Timeout = _smtpSettings.Timeout;

                if (!string.IsNullOrEmpty(_smtpSettings.Username))
                {
                    await client.AuthenticateAsync(_smtpSettings.Username, _smtpSettings.Password);
                }
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
        }
    }
}
