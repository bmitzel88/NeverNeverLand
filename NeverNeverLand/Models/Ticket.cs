using System.ComponentModel.DataAnnotations;

namespace NeverNeverLand.Models
{
    public class Ticket
    {
        public int TicketId { get; set; }

        [Required]
        public required string UserId { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime PurchaseDate { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime ExpirationDate { get; set; }

        public bool IsUsed { get; set; }
    }
}
