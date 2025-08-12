namespace NeverNeverLand.Models
{
    public class PassViewModel
    {
        public decimal CurrentPrice { get; set; }
        public string PhaseLabel { get; set; } = "";    // "Early Bird", "Regular", "Late-Season"
        public string Blurb { get; set; } = "";         // short description string
    }
}
