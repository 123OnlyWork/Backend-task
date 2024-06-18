using MailKit.Net.Smtp;
using Microsoft.Extensions.Logging;
using MimeKit;
using System;

namespace SendingEmail.Services
{
    public interface IEmailService
    {
        EmailSendResult SendEmail(string toEmail, string subject, string body);
    }

    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;

        public EmailService(ILogger<EmailService> logger)
        {
            _logger = logger;
        }

        public EmailSendResult SendEmail(string toEmail, string subject, string body)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("Тема письма", "escripko150101@gmail.com"));
                message.To.Add(new MailboxAddress("", toEmail));
                message.Subject = subject;
                message.Body = new TextPart(MimeKit.Text.TextFormat.Html)
                {
                    Text = body
                };

                using (var client = new SmtpClient())
                {
                    client.Connect("smtp.gmail.com", 587, false);
                    client.Authenticate("escripko150101@gmail.com", "corj dvcl cuhq iuxk");

                    client.Send(message);
                    client.Disconnect(true);

                    return new EmailSendResult { Success = true };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка отправки письма: {ex.Message}");
                return new EmailSendResult { Success = false, ErrorMessage = "Ошибка отправки письма." };
            }
        }
    }

    public class EmailSendResult
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
    }
}
