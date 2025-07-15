namespace NeverNeverLand.Models
{
    public class CartItem
    {
        public int Id { get; set; } // Unique ID for the cart item
        public required string ItemType { get; set; } // "Ticket", "Membership", "Product"
        public required string Name { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}
