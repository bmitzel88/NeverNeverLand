namespace NeverNeverLand.Services
{
    public interface IEmailService
    {
        Task SendTicketAsync(string email, string subject, string htmlContent);
    }
}
