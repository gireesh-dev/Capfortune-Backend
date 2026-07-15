using Resend;

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
            _fromEmail = _configuration["ResendSettings:FromEmail"]!;
            _fromName = _configuration["ResendSettings:FromName"]!;
        }

        public async Task<bool> SendMailAsync(string toEmail, string toName, string subject, string htmlBody)
        {
            try
            {
                var message = new EmailMessage
                {
                    From = _fromEmail,  
                    To = { toEmail }, 
                    Subject = subject,
                    HtmlBody = htmlBody
                };

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