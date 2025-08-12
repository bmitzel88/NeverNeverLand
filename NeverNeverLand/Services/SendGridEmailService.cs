using System.Text;
using SendGrid;
using SendGrid.Helpers.Mail;


namespace NeverNeverLand.Services.SendGridEmailService
{
    public class SendGridEmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public SendGridEmailService(IConfiguration config)
        {
            _config = config;
        }

        public Task SendTicketAsync(string toEmail, string subject, string htmlContent)
            => SendAsync(toEmail, subject, htmlContent);

        // If you kept this in the interface, keep it here; otherwise remove both.
        public Task SendPassAsync(string toEmail, string subject, string htmlContent)
            => SendAsync(toEmail, subject, htmlContent);

        private async Task SendAsync(string toEmail, string subject, string htmlContent)
        {
            var apiKey = _config["SendGrid:ApiKey"];
            if (string.IsNullOrWhiteSpace(apiKey))
                throw new InvalidOperationException("Missing SendGrid:ApiKey");

            var from = new EmailAddress(
                _config["SendGrid:FromEmail"] ?? "noreply@neverneverland.com",
                _config["SendGrid:FromName"] ?? "Never Never Land"
            );

            var to = new EmailAddress(toEmail);
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent: null, htmlContent);

            var response = await new SendGridClient(apiKey).SendEmailAsync(msg);
            if ((int)response.StatusCode < 200 || (int)response.StatusCode >= 300)
                throw new ApplicationException($"SendGrid failed: {response.StatusCode}. Body: {response.Body}");
        }
    }
}