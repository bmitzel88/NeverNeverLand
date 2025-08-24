using System.Net.Sockets;

namespace NeverNeverLand.Models
{
    public class Price
    {
        public int Id { get; set; }

        public int SeasonId { get; set; }
        public Season Season { get; set; } = null!;

        public string AdmissionType { get; set; } = ""; // e.g., "Adult", "Child",  probably in the future add "Senior"
        public string Channel { get; set; } = "Online";      // "Online" or "Gate"

        public decimal Amount { get; set; }
        public string Currency { get; set; } = "USD";

        public DateTime? EffectiveStartUtc { get; set; }
        public DateTime? EffectiveEndUtc { get; set; }

        public bool IsActive { get; set; } = true;
    }

}
