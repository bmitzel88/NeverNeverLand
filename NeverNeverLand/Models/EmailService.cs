using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace NeverNeverLand.Models
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendTicketAsync(string recipientEmail)
        {
            var apiKey = _configuration["SendGrid:ApiKey"];
            var client = new SendGridClient(apiKey);

            var from = new EmailAddress("tickets@neverneverland.com", "Never Never Land");
            var to = new EmailAddress(recipientEmail);
            var subject = "Your Never Never Land Ticket";
            var plainTextContent = "Thanks for your purchase! This email confirms your ticket to Never Never Land.";
            var htmlContent = "<strong>Thanks for your purchase!</strong><br>Your ticket is attached or will be available in your account.";

            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
            var response = await client.SendEmailAsync(msg);
        }
    }
}
