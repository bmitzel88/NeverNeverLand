namespace NeverNeverLand.Models
{
    public class ParkPass
    {
        public int Id { get; set; } // Unique identifier for the park pass
        public string Name { get; set; } // Name/Type of the park pass
        public string Description { get; set; } // Description of the park pass
        public decimal Price { get; set; } // Price of the park pass
        public DateTime ValidFrom { get; set; } // Start date of validity
        public DateTime ValidUntil { get; set; } // End date of validity
        public bool IsActive { get; set; } // Indicates if the park pass is currently active
    }
}
