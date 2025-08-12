namespace NeverNeverLand.Models
{
    public class PassBuyViewModel
    {
        public string PhaseLabel { get; set; } = "";
        public decimal CurrentPrice { get; set; }
        public string PublishableKey { get; set; } = "";
        public IEnumerable<string> PassTypes { get; set; } = new[] { "Personal", "Family", "Family+" };
    }

}
