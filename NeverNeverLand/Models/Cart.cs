using System.Collections.Generic;
using System.Linq;

namespace NeverNeverLand.Models
{
    public class Cart
    {
        public List<CartItem> Items { get; set; } = new();

        public decimal Total => Items.Sum(i => i.Price * i.Quantity);

        public void AddItem(CartItem item)
        {
            var existing = Items.FirstOrDefault(i => i.Id == item.Id && i.ItemType == item.ItemType);
            if (existing != null)
            {
                existing.Quantity += item.Quantity;
            }
            else
            {
                Items.Add(item);
            }
        }

        public void RemoveItem(int id)
        {
            Items.RemoveAll(i => i.Id == id);
        }

        public void Clear()
        {
            Items.Clear();
        }
    }
}