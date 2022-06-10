using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Text;
using PBL3.DTO;

namespace PBL3.Service {
    public class EmailService : IEmailService {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config) {
            _config = config;
        }
        public async Task SendEmail(EmailDto emailDto) {
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(_config.GetSection("SMTP:EmailUserName").Value));
            email.To.Add(MailboxAddress.Parse(emailDto.To));
            email.Subject = emailDto.Subject;
            email.Body = new TextPart(TextFormat.Html) { Text = emailDto.Key };

            using var smtp = new SmtpClient();
            smtp.Connect(_config.GetSection("SMTP:EmailHost").Value, 587, SecureSocketOptions.StartTls);
            smtp.Authenticate(_config.GetSection("SMTP:EmailUserName").Value, _config.GetSection("SMTP:EmailPassword").Value);
            await smtp.SendAsync(email);
            smtp.Disconnect(true);
        }
    }
}
