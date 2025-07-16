using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NeverNeverLand.Models;

namespace NeverNeverLand.Areas.Account.Pages
{
    public class CartModel : PageModel
    {
        public Cart Cart { get; set; } = new();


        public void OnGet()
        {
            Cart = HttpContext.Session.GetObject<Cart>("Cart") ?? new Cart();
        }

        public IActionResult OnPostRemove(int id)
        {
            Cart = HttpContext.Session.GetObject<Cart>("Cart") ?? new Cart();
            Cart.RemoveItem(id);
            HttpContext.Session.SetObject("Cart", Cart);
            return RedirectToPage();
        }
    }
}
