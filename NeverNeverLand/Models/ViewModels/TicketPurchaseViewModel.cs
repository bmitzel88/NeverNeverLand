using System.ComponentModel.DataAnnotations;

namespace NeverNeverLand.Models.ViewModels
{
    public class TicketPurchaseViewModel
    {
        [Required]
        public string HolderName { get; set; } = "";

        [Range(0, 120)]
        public int HolderAge { get; set; }

        // Calculated during purchase
        public string AdmissionType { get; set; } = "";
        public decimal PricePaid { get; set; }
    }

}
