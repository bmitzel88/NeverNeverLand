using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Areas.Identity.Pages.Account.Manage
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        public IndexModel(UserManager<IdentityUser> userManager) => _userManager = userManager;

        // Portal data
        public ParkPassSummary? Pass { get; set; }
        public List<TicketSummary> Tickets { get; set; } = new();
        public DateTime SeasonStart { get; set; }
        public DateTime SeasonEnd { get; set; }
        public string? PortalMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null) return NotFound("Unable to load user.");

            (SeasonStart, SeasonEnd) = CurrentSeason();

            // TODO: load real data for user; sample data for layout:
            Pass = null; // no pass by default

            Tickets = new List<TicketSummary>
            {
                new TicketSummary { Id=Guid.NewGuid(), Season=SeasonStart.Year, PurchasedOn=SeasonStart.AddDays(12), ExpiresOn=SeasonEnd, RedeemedOn=null },
                new TicketSummary { Id=Guid.NewGuid(), Season=SeasonStart.Year, PurchasedOn=SeasonStart.AddDays(20), ExpiresOn=SeasonEnd, RedeemedOn=SeasonStart.AddDays(45) },
                new TicketSummary { Id=Guid.NewGuid(), Season=SeasonStart.Year-1, PurchasedOn=new DateTime(SeasonStart.Year-1,6,10), ExpiresOn=new DateTime(SeasonStart.Year-1,10,31), RedeemedOn=null }
            }
            .OrderByDescending(t => t.Season).ThenByDescending(t => t.PurchasedOn).ToList();

            if (TempData.TryGetValue("PortalMessage", out var msg)) PortalMessage = msg?.ToString();
            return Page();
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostEraseTicketAsync(Guid id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null) return NotFound("Unable to load user.");

            // TODO: verify ticket belongs to user and is ExpiredUnused, then delete in DB
            TempData["PortalMessage"] = "Ticket erased.";
            return RedirectToPage();
        }

        private static (DateTime start, DateTime end) CurrentSeason()
        {
            var y = DateTime.UtcNow.Year;
            return (new DateTime(y, 4, 1), new DateTime(y, 10, 31));
        }

        // Simple DTOs
        public class ParkPassSummary
        {
            public string PassNumber { get; set; } = "";
            public string TypeName { get; set; } = "Season Pass";
            public DateTime ValidFrom { get; set; }
            public DateTime ValidTo { get; set; }
            public int GuestAllowance { get; set; }
            public bool IsActive => DateTime.UtcNow.Date <= ValidTo.Date;
        }
        public class TicketSummary
        {
            public Guid Id { get; set; }
            public int Season { get; set; }
            public DateTime PurchasedOn { get; set; }
            public DateTime ExpiresOn { get; set; }
            public DateTime? RedeemedOn { get; set; }
            public TicketState State =>
                RedeemedOn.HasValue ? TicketState.Used :
                (DateTime.UtcNow.Date > ExpiresOn.Date ? TicketState.ExpiredUnused : TicketState.Valid);
        }
        public enum TicketState { Valid, Used, ExpiredUnused }
    }
}
