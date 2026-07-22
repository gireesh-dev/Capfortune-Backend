using Resend;
using System;
using System.Linq;

namespace CapfortuneBE.Service
{
    public class MailService
    {
        private readonly IResend _resend;
        private readonly ILogger<MailService> _logger;
        private readonly IConfiguration _configuration;
        private readonly string _fromEmail;
        private readonly string _fromName;

        public MailService(IResend resend, ILogger<MailService> logger, IConfiguration configuration)
        {
            _resend = resend;
            _logger = logger;
            _configuration = configuration;
            _fromEmail = _configuration["ResendSettings:FromEmail"] ?? "onboarding@resend.dev";
            _fromName = _configuration["ResendSettings:FromName"] ?? "Capfortune";
        }

        public async Task<bool> SendMailAsync(string toEmail, string toName, string subject, string htmlBody, string? bccEmail = null)
        {
            var recipients = (toEmail ?? string.Empty)
                .Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .ToList();

            if (recipients.Count == 0)
            {
                _logger.LogWarning("Skipped sending email '{Subject}' because the recipient address was empty.", subject);
                return false;
            }

            var bccRecipients = (bccEmail ?? string.Empty)
                .Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .ToList();

            try
            {
                var message = new EmailMessage
                {
                    From = _fromEmail,
                    Subject = subject,
                    HtmlBody = htmlBody
                };

                foreach (var recipient in recipients)
                {
                    message.To.Add(recipient);
                }

                if (bccRecipients.Count > 0)
                {
                    message.Bcc ??= new EmailAddressList();
                    foreach (var bccRecipient in bccRecipients)
                    {
                        message.Bcc.Add(bccRecipient);
                    }
                }

                var response = await _resend.EmailSendAsync(message);

                if (response.Success)
                {
                    _logger.LogInformation("Email sent successfully to {ToEmail}", toEmail);
                    return true;
                }

                _logger.LogWarning("Failed to send email to {ToEmail}", toEmail);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while sending email to {ToEmail}", toEmail);
                throw;
            }
        }
    }
}