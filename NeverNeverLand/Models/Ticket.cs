using System.ComponentModel.DataAnnotations;
using System.Composition.Convention;

namespace NeverNeverLand.Models
{
    public class Ticket
    {
        [Key]
        public int TicketId { get; set; }

        // This ties the purchase back to the buyer's account (parent/guardian)
        [Required]
        public string UserId { get; set; } = ""; 

        // Info about the actual ticket holder
        [Required, MaxLength(100)]
        public string HolderName { get; set; } = "";   

        [Range(0, 120)]
        public int HolderAge { get; set; }             // used to validate Child/Adult pricing

        [Required, MaxLength(50)]
        public string AdmissionType { get; set; } = ""; // "Adult", "Child"

        [DataType(DataType.Currency)]
        public decimal PricePaid { get; set; } // for keeping records of what was paid

        public string Currency { get; set; } = "USD";

        [DataType(DataType.DateTime)]
        public DateTime PurchaseDate { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime ExpirationDate { get; set; }

        public bool IsUsed { get; set; }
    }
}
