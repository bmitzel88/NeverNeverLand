namespace NeverNeverLand.Models
{
    public class Price
    {
        public int Id { get; set; }

        public int SeasonId { get; set; }
        public Season Season { get; set; } = null!;

        // "Ticket", "Pass", "Product"
        public string Kind { get; set; } = "Ticket";

        // Variant, e.g. "Adult", "Child", "PersonalPass", "Hoodie"
        public string Item { get; set; } = "";

        // Where it’s sold
        public string Channel { get; set; } = "Online"; // "Online" or "Gate"

        public decimal Amount { get; set; }
        public string Currency { get; set; } = "USD";

        public DateTime? EffectiveStartUtc { get; set; }
        public DateTime? EffectiveEndUtc { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
