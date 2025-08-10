using NeverNeverLand.Services;
using SendGrid;
using SendGrid.Helpers.Mail;


public class SendGridEmailService : IEmailService
{
    private readonly IConfiguration _config; // Configuration to access SendGrid API key

    public SendGridEmailService(IConfiguration config) // Constructor to inject configuration
    {
        _config = config;
    }

    
    public async Task SendTicketAsync(string toEmail, string subject, string htmlContent) // Method to send an email with a ticket
    {
        var apiKey = _config["SendGrid:ApiKey"];
        var client = new SendGridClient(apiKey);

        var from = new EmailAddress("tickets@neverneverland.com", "Never Never Land");
        var to = new EmailAddress(toEmail);
        var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent: null, htmlContent);

        await client.SendEmailAsync(msg);
    }
}
