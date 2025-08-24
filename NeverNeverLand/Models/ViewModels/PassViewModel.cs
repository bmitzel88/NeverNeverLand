namespace NeverNeverLand.Models.ViewModels
{
    public class PassViewModel
    {
        public decimal CurrentPrice { get; set; }
        public string PhaseLabel { get; set; } = "";    // "Early Bird", "Regular", "Late-Season"
        public string Blurb { get; set; } = "";         // short description string


        public decimal PersonalPrice { get; set; }
        public decimal FamilyPrice { get; set; }
        public decimal FamilyPlusPrice { get; set; }
    }
}
