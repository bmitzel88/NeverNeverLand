namespace NeverNeverLand.Models.ViewModels
{
    using System.ComponentModel.DataAnnotations;
    using Microsoft.AspNetCore.Mvc.Rendering;

    public class AdminPricingViewModel
    {
        // Create/Update form
        public int SelectedSeasonId { get; set; }
        public string SelectedKind { get; set; } = "Ticket"; // "Ticket" | "Pass"
        public string SelectedAdmissionType { get; set; } = "Adult";
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "USD";

        // Options
        public List<SelectListItem> SeasonOptions { get; set; } = new();
        public List<SelectListItem> KindOptions { get; set; } = new()
        {
            new("Ticket", "Ticket"),
            new("Pass", "Pass")
        };
        public List<SelectListItem> AdmissionTypeOptions { get; set; } = new();

        // List + filters
        public IEnumerable<Price> Prices { get; set; } = new List<Price>();
        public int? FilterSeasonId { get; set; }
        public string FilterKind { get; set; } = "All"; // All | Ticket | Pass
        public string? FilterItem { get; set; }         // e.g., Adult/Child/Infant | Personal/Family/Family+
        public bool IncludeInactive { get; set; } = false;
        public string? Q { get; set; }                  // search
    }
}
