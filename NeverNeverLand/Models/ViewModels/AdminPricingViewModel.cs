namespace NeverNeverLand.Models.ViewModels
{
    using System.ComponentModel.DataAnnotations;
    using Microsoft.AspNetCore.Mvc.Rendering;

    public class AdminPricingViewModel
    {
        // Form fields
        [Required] public int SelectedSeasonId { get; set; }
        [Required] public string SelectedAdmissionType { get; set; } = "Adult";
        [Range(0, 999999)] public decimal Amount { get; set; }
        [Required, StringLength(3)] public string Currency { get; set; } = "USD";

        // Dropdown data
        public List<SelectListItem> SeasonOptions { get; set; } = new();
        public List<SelectListItem> AdmissionTypeOptions { get; set; } = new();


        // Table data
        public List<Price> Prices { get; set; } = new();
    }
}
