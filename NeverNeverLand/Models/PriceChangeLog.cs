namespace NeverNeverLand.Models
{
    public class PriceChangeLog
    {
        public int Id { get; set; }

        // Context of the change
        public int TicketId { get; set; }         // which ticket this price applied to
        public int? SeasonId { get; set; }        // which season (nullable if AlwaysOn/default)

        // Audit info
        public string ChangedByUserId { get; set; } = "";
        public DateTime ChangedAtUtc { get; set; } = DateTime.UtcNow;

        // Before/after snapshot
        public decimal OldAmount { get; set; }
        public decimal NewAmount { get; set; }
        public string Currency { get; set; } = "USD";

        public string? Note { get; set; }
    }
}
