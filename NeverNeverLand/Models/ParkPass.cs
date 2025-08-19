namespace NeverNeverLand.Models
{
    public class ParkPass
    {
        public Guid Id { get; set; }
        public string Type { get; set; } = "Personal"; // Personal | Family | Family+
        public int SeasonYear { get; set; }
        public DateTime ExpiresAt { get; set; }
        public string Status { get; set; } = "Active";
        public string QrToken { get; set; } = Guid.NewGuid().ToString("N");

        public int MaxOwners { get; set; } // Personal=1, Family=2, Family+=2
        public int MaxGuests { get; set; } // Personal=2, Family=4, Family+=7
    }
}