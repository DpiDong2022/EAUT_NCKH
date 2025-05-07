
using MailKit.Net.Smtp;
using MimeKit;

namespace EAUT_NCKH.Web.Services {
    public class EmailService {
        private readonly string smtpHost;
        private readonly int smtpPort;
        private readonly string smtpUser;
        private readonly string smtpPass;
        private readonly string fromName;
        private readonly bool isSendToRealEmail;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _env;

        public EmailService(IConfiguration configuration, IWebHostEnvironment env) {
            _configuration = configuration;
            _env = env;
            this.smtpHost = _configuration["EmailSettings:smtpHost"];
            this.smtpPort = _configuration.GetValue<int>("EmailSettings:smtpPort");
            this.smtpUser = _configuration["EmailSettings:smtpUser"];
            this.smtpPass = _configuration["EmailSettings:smtpPass"];
            this.fromName = _configuration["EmailSettings:fromName"];
            this.isSendToRealEmail = _configuration.GetValue<bool>("EmailSettings:isSendToRealEmail");
        }

        public async Task SendEmailAsync(List<string> toEmails, string subject, string body, bool isHtml = true) {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(fromName, smtpUser));

            foreach (var email in toEmails) {
                if (!string.IsNullOrWhiteSpace(email)) {
                    if (!isSendToRealEmail) {
                        message.To.Add(MailboxAddress.Parse(smtpUser));
                    } else {
                        message.To.Add(MailboxAddress.Parse(email.Trim()));
                    }
                }
            }

            message.Subject = subject;

            var bodyBuilder = new BodyBuilder();
            if (isHtml) {
                bodyBuilder.HtmlBody = body;
            } else {
                bodyBuilder.TextBody = body;
            }

            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(smtpHost, smtpPort, MailKit.Security.SecureSocketOptions.SslOnConnect);
            await client.AuthenticateAsync(smtpUser, smtpPass);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);

        }

        public string GetEmailTemplate(string fileName) {
            var filePath = Path.Combine("EmailTemplates", fileName);
            string html = File.ReadAllText(filePath);

            return html;
        }
    }
}
