using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NeverNeverLand.Data;
using NeverNeverLand.Models;
using Stripe;
using Stripe.Checkout;

namespace NeverNeverLand.Controllers
{
    public class ParkPassesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly StripeSettings _stripe;

        // Season runs April 1 – Oct 31 
        private static readonly DateTime SeasonStart = new(DateTime.Now.Year, 4, 1);
        private static readonly DateTime SeasonEnd = new(DateTime.Now.Year, 10, 31);

        public ParkPassesController(ApplicationDbContext context, IOptions<StripeSettings> stripeOptions)
        {
            _context = context;
            _stripe = stripeOptions.Value; // Secret & Publishable from config/secrets
        }


        /// <summary>
        /// ParkPasses/Index
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            var today = DateTime.Today;

            // Pricing 
            const decimal earlyBird = 90m;   // Jan 1 – Mar 31
            const decimal regular = 120m;  // Apr 1 – Jul 31
            const decimal late = 96m;   // Aug 1 – Oct 31

            string phase;
            decimal price;
            string blurb;

            // Determine current phase and price
            var year = today.Year;
            var ebStart = new DateTime(year, 1, 1);
            var ebEnd = new DateTime(year, 3, 31);
            var regEnd = new DateTime(year, 7, 31);
            var lateEnd = new DateTime(year, 10, 31);

            if (today >= ebStart && today <= ebEnd)
            {
                phase = "Early Bird";
                price = earlyBird;
                blurb = "Buy now and lock in the lowest price for the season.";
            }
            else if (today >= SeasonStart && today <= regEnd)
            {
                phase = "Regular";
                price = regular;
                blurb = "Enjoy unlimited visits all season long.";
            }
            else if (today >= new DateTime(year, 8, 1) && today <= lateEnd)
            {
                phase = "Late-Season";
                price = late;
                blurb = "Join now and enjoy the rest of the season at a reduced price.";
            }
            else
            {
                // Off-season: hide price / disable buy if you prefer
                phase = "Off-Season";
                price = regular; // or 0m
                blurb = "Season passes will be available again before next season.";
            }

            var vm = new PassViewModel
            {
                CurrentPrice = price,
                PhaseLabel = phase,
                Blurb = blurb
            };

            return View(vm);
        }

        /// <summary>
        /// Determines the current phase and price based on today's date.
        /// </summary>
        /// <returns>correct price depending on the current phase of the season</returns>
        private (string phase, decimal price) GetPhasePrice()
        {
            var y = DateTime.Today.Year;
            var today = DateTime.Today;
            var earlyB = (new DateTime(y, 1, 1), new DateTime(y, 3, 31), 90m, "Early Bird");
            var regulr = (SeasonStart, new DateTime(y, 7, 31), 120m, "Regular");
            var late = (new DateTime(y, 8, 1), new DateTime(y, 10, 31), 96m, "Late-Season");

            if (today >= earlyB.Item1 && today <= earlyB.Item2) return (earlyB.Item4, earlyB.Item3);
            if (today >= regulr.Item1 && today <= regulr.Item2) return (regulr.Item4, regulr.Item3);
            if (today >= late.Item1 && today <= late.Item2) return (late.Item4, late.Item3);

            return ("Off-Season", regulr.Item3);
        }


        /// <summary>
        /// ParkPasses/Buy
        /// </summary>
        /// <returns>ParkPasses/Buy view</returns>
        public IActionResult Buy()
        {
            var (phase, price) = GetPhasePrice();
            var vm = new PassBuyViewModel
            {
                PhaseLabel = phase,
                CurrentPrice = price,
                PublishableKey = _stripe.PublishableKey
            };
            return View(vm);
        }

        // POST: /Pass/CreatePaymentIntent  (called by fetch from the view)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePaymentIntent([FromBody] PassPurchaseRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.PassType))
                return BadRequest("Missing email or pass type.");

            StripeConfiguration.ApiKey = _stripe.SecretKey;

            // If different pass types have different prices, map them here.
            var (phase, basePrice) = GetPhasePrice();
            var passTypeMultiplier = req.PassType switch
            {
                "Personal" => 1.0m,
                "Family" => 1.25m,
                "Family+" => 1.6m,
                _ => 1.0m
            };
            var finalPrice = basePrice * passTypeMultiplier;          // decimal dollars
            var amount = (long)Math.Round(finalPrice * 100M);          // cents

            var options = new PaymentIntentCreateOptions
            {
                Amount = amount,
                Currency = "usd",
                AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                {
                    Enabled = true
                },
                ReceiptEmail = req.Email,
                Metadata = new Dictionary<string, string>
                {
                    ["nnl_item"] = "SeasonPass",
                    ["nnl_pass_type"] = req.PassType,
                    ["nnl_phase"] = phase
                }
            };

            var service = new PaymentIntentService();
            var intent = await service.CreateAsync(options);

            return Json(new { clientSecret = intent.ClientSecret });
        }

        // GET: /Pass/Success
        public IActionResult Success() => View();

        // DTO used by CreatePaymentIntent
        public class PassPurchaseRequest
        {
            public string PassType { get; set; } = "";
            public string Email { get; set; } = "";
        }





        /// 
        /// ADMIN PANEL
        /// 



        // GET: ParkPasses/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var parkPass = await _context.ParkPass
                .FirstOrDefaultAsync(m => m.Id == id);
            if (parkPass == null)
            {
                return NotFound();
            }

            return View(parkPass);
        }


        // GET: ParkPasses/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: ParkPasses/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Description,Price,ValidFrom,ValidUntil,IsActive")] ParkPass parkPass)
        {
            if (ModelState.IsValid)
            {
                _context.Add(parkPass);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(parkPass);
        }

        // GET: ParkPasses/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var parkPass = await _context.ParkPass.FindAsync(id);
            if (parkPass == null)
            {
                return NotFound();
            }
            return View(parkPass);
        }

        // POST: ParkPasses/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,Price,ValidFrom,ValidUntil,IsActive")] ParkPass parkPass)
        {
            if (id != parkPass.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(parkPass);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ParkPassExists(parkPass.Id))
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
            return View(parkPass);
        }

        // GET: ParkPasses/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var parkPass = await _context.ParkPass
                .FirstOrDefaultAsync(m => m.Id == id);
            if (parkPass == null)
            {
                return NotFound();
            }

            return View(parkPass);
        }

        // POST: ParkPasses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var parkPass = await _context.ParkPass.FindAsync(id);
            if (parkPass != null)
            {
                _context.ParkPass.Remove(parkPass);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ParkPassExists(int id)
        {
            return _context.ParkPass.Any(e => e.Id == id);
        }
    }

}
