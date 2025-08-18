namespace NeverNeverLand.Models
{
    public class PassBuyViewModel
    {
        public string PhaseLabel { get; set; } = "";
        public decimal CurrentPrice { get; set; }
        public string PublishableKey { get; set; } = "";
        public IEnumerable<string> PassTypes { get; set; } = new[] { "Personal", "Family", "Family+" };

        // Which pass the user is buying
        public string SelectedPassType { get; set; } = "Personal";

        
        public decimal PersonalPrice { get; set; }
        public decimal FamilyPrice { get; set; }
        public decimal FamilyPlusPrice { get; set; }
    }

}
