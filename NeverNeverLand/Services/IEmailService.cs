using System.Threading.Tasks;

namespace NeverNeverLand.Services
{
    public interface IEmailService
    {
        Task SendTicketAsync(string toEmail, string subject, string htmlContent);
        Task SendPassAsync(string toEmail, string htmlContent, string subject); 
    }
}
