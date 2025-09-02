using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NeverNeverLand.Data;
using NeverNeverLand.Models;
using Stripe;
using Stripe.Checkout;

namespace NeverNeverLand.Controllers
{
    public class TicketsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly StripeSettings _stripeSettings;

        public TicketsController(ApplicationDbContext context, IOptions<StripeSettings> stripeOptions)
        {
            _context = context;
            _stripeSettings = stripeOptions.Value;
        }


        // GET: Tickets
        public async Task<IActionResult> Index()
        {
            return View(await _context.Ticket.ToListAsync());
        }
        // GET: Tickets/Buy
        public IActionResult Buy()
        {
            return View(); 
        }

        // POST: Tickets/CreateCheckoutSession
        [HttpPost]
        public async Task<IActionResult> CreateCheckoutSession(string email, int adultQty, int childQty)
        {
            if (string.IsNullOrEmpty(email) || (adultQty + childQty) == 0)
            {
                ModelState.AddModelError("", "Please provide an email and select at least one ticket.");
                return View("Buy");
            }

            // 1) Pick the season: prefer AlwaysOn; otherwise use the dated season covering today
            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            var seasonId = await _context.Seasons
                .Where(s => s.IsActive && s.AlwaysOn)
                .Select(s => s.Id)
                .FirstOrDefaultAsync();

            if (seasonId == 0)
            {
                seasonId = await _context.Seasons
                    .Where(s => s.IsActive &&
                                s.StartDate <= today &&
                                (s.EndDate == null || s.EndDate >= today))
                    .OrderBy(s => s.StartDate)
                    .Select(s => s.Id)
                    .FirstOrDefaultAsync();

                if (seasonId == 0)
                {
                    ModelState.AddModelError("", "No active season configured. Ask an admin to add one in Pricing Admin.");
                    return View("Buy");
                }
            }

            // 2) Fetch current active prices for Adult/Child
            decimal adultPrice = 0m, childPrice = 0m;

            if (adultQty > 0)
            {
                adultPrice = await _context.Prices
                    .Where(p => p.SeasonId == seasonId && p.Item == "Adult" && p.IsActive)
                    .OrderByDescending(p => p.EffectiveStartUtc)
                    .Select(p => p.Amount)
                    .FirstOrDefaultAsync();
                if (adultPrice <= 0m)
                {
                    ModelState.AddModelError("", "Adult ticket price is not configured.");
                    return View("Buy");
                }
            }

            if (childQty > 0)
            {
                childPrice = await _context.Prices
                    .Where(p => p.SeasonId == seasonId && p.Item == "Child" && p.IsActive)
                    .OrderByDescending(p => p.EffectiveStartUtc)
                    .Select(p => p.Amount)
                    .FirstOrDefaultAsync();
                if (childPrice < 0m) // allow 0 if you have free child tickets
                {
                    ModelState.AddModelError("", "Child ticket price is not configured.");
                    return View("Buy");
                }
            }

            // 3) Build Stripe line items from DB prices
            StripeConfiguration.ApiKey = _stripeSettings.SecretKey;

            var lineItems = new List<SessionLineItemOptions>();

            if (adultQty > 0)
            {
                lineItems.Add(new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)decimal.Round(adultPrice * 100m, 0, MidpointRounding.AwayFromZero),
                        Currency = "usd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions { Name = "Adult Ticket" }
                    },
                    Quantity = adultQty
                });
            }

            if (childQty > 0)
            {
                lineItems.Add(new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)decimal.Round(childPrice * 100m, 0, MidpointRounding.AwayFromZero),
                        Currency = "usd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions { Name = "Child Ticket" }
                    },
                    Quantity = childQty
                });
            }

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = lineItems,
                CustomerEmail = email,
                Mode = "payment",
                SuccessUrl = Url.Action("Success", "Tickets", null, Request.Scheme),
                CancelUrl = Url.Action("Buy", "Tickets", null, Request.Scheme)
            };

            var service = new SessionService();
            var session = service.Create(options);
            return Redirect(session.Url);
        }

        public async Task<IActionResult> Success()
        {
            // Save ticket(s) for the logged-in user after successful payment
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
            if (user == null)
                return Challenge();

            // Get current season end date
            var now = DateTime.UtcNow;
            var season = await _context.Seasons
                .Where(s => s.IsActive &&
                    ((s.StartDate == null || s.StartDate.Value.ToDateTime(TimeOnly.MinValue) <= now) &&
                     (s.EndDate == null || s.EndDate.Value.ToDateTime(TimeOnly.MinValue) >= now)))
                .OrderBy(s => s.StartDate)
                .FirstOrDefaultAsync();
            var seasonEnd = season?.EndDate?.ToDateTime(TimeOnly.MinValue) ?? new DateTime(now.Year, 10, 31);

            // For demo: create a single ticket. In production, you should know the quantity/type from session or payment metadata.
            var ticket = new Ticket
            {
                UserId = user.Id,
                HolderName = user.UserName ?? "Member",
                HolderAge = 0, // Set appropriately if you collect this info
                AdmissionType = "Adult", // Or "Child" as appropriate
                PricePaid = 20.00m, // Set from purchase
                Currency = "USD",
                PurchaseDate = now,
                ExpirationDate = seasonEnd, // Always set to last day of season
                IsUsed = false
            };

            _context.Ticket.Add(ticket);
            await _context.SaveChangesAsync();

            return View(); // Basic thank-you page
        }


        //
        // ADMIN PANEL
        //


        // GET: Ticket Management (Mostly Admin Only)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Manage()
        {
            return View(await _context.Ticket.ToListAsync());
        }

        // GET: Tickets/Details/1
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticket = await _context.Ticket
                .FirstOrDefaultAsync(m => m.TicketId == id);
            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }

        // GET: Tickets/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Tickets/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("TicketId,UserId,PurchaseDate,ExpirationDate,IsUsed")] Ticket ticket)
        {
            if (ModelState.IsValid)
            {
                _context.Add(ticket);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(ticket);
        }

        // GET: Tickets/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticket = await _context.Ticket.FindAsync(id);
            if (ticket == null)
            {
                return NotFound();
            }
            return View(ticket);
        }

        // POST: Tickets/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("TicketId,UserId,PurchaseDate,ExpirationDate,IsUsed")] Ticket ticket)
        {
            if (id != ticket.TicketId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(ticket);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TicketExists(ticket.TicketId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(ticket);
        }

        // GET: Tickets/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticket = await _context.Ticket
                .FirstOrDefaultAsync(m => m.TicketId == id);
            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }

        // POST: Tickets/Delete/5
        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ticket = await _context.Ticket.FindAsync(id);
            if (ticket != null)
            {
                _context.Ticket.Remove(ticket);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        /// <summary>
        /// Checks if a ticket exists in the database.
        /// </summary>
        /// <param name="id"></param>
        /// <returns>True if ticket exists, false if not</returns>

        private bool TicketExists(int id)
        {
            return _context.Ticket.Any(e => e.TicketId == id);
        }
    }
}
