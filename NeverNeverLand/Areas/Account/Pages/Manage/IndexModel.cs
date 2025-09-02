using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NeverNeverLand.Data;
using NeverNeverLand.Models;
using Microsoft.EntityFrameworkCore;

namespace Areas.Identity.Pages.Account.Manage
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ApplicationDbContext _db;
        public IndexModel(UserManager<IdentityUser> userManager, ApplicationDbContext db)
        {
            _userManager = userManager;
            _db = db;
        }

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

            // Load real park pass for this user (active for current season)
            var pass = await _db.ParkPass
                .Where(p => p.UserId == user.Id && p.SeasonYear == SeasonStart.Year && p.Status == "Active")
                .OrderByDescending(p => p.ExpiresAt)
                .FirstOrDefaultAsync();

            if (pass != null)
            {
                Pass = new ParkPassSummary
                {
                    PassNumber = pass.QrToken,
                    TypeName = pass.Type + " Pass",
                    ValidFrom = new DateTime(pass.SeasonYear, 4, 1),
                    ValidTo = pass.ExpiresAt,
                    GuestAllowance = pass.MaxGuests
                };
            }
            else
            {
                Pass = null;
            }

            // Load real tickets for this user
            var userTickets = await _db.Ticket
                .Where(t => t.UserId == user.Id)
                .OrderByDescending(t => t.PurchaseDate)
                .ToListAsync();

            Tickets = userTickets.Select(t => new TicketSummary
            {
                Id = t.TicketId, // Use the real TicketId
                Season = SeasonStart.Year, // Or use a season property if available
                PurchasedOn = t.PurchaseDate,
                ExpiresOn = t.ExpirationDate,
                RedeemedOn = t.IsUsed ? t.ExpirationDate : (DateTime?)null // Or use a real RedeemedOn if you have it
            }).ToList();

            if (TempData.TryGetValue("PortalMessage", out var msg)) PortalMessage = msg?.ToString();
            return Page();
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostEraseTicketAsync(int id)
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
            public int Id { get; set; } // Use TicketId (int)
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
