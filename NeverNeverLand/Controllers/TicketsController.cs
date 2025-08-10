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
        public IActionResult CreateCheckoutSession(string email, int adultQty, int childQty)
        {

            if (string.IsNullOrEmpty(email) || (adultQty + childQty) == 0)
            {
                ModelState.AddModelError("", "Please provide an email and select at least one ticket.");
                return View("Buy"); // This will return users back to the Buy form with an error message
            }


            StripeConfiguration.ApiKey = _stripeSettings.SecretKey;

            var lineItems = new List<SessionLineItemOptions>();

            if (adultQty > 0)
            {
                lineItems.Add(new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = 800,
                        Currency = "usd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = "Adult Ticket"
                        }
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
                        UnitAmount = 400,
                        Currency = "usd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = "Child Ticket"
                        }
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

        public IActionResult Success()
        {
            return View(); // Basic thank-you page
        }


        //
        // ADMIN PANEL
        //


        // GET: Ticket Management (Admin Only)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Manage()
        {
            return View(await _context.Ticket.ToListAsync());
        }

        // GET: Tickets/Details/1
        [Authorize(Roles = "Admin")]
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
