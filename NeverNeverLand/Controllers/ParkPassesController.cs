using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NeverNeverLand.Data;
using NeverNeverLand.Models;
using NeverNeverLand.Services;
using Stripe;

namespace NeverNeverLand.Controllers
{
    public class ParkPassesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly StripeSettings _stripe;
        private readonly IPriceService _pricing;

        public ParkPassesController(
            ApplicationDbContext context,
            IOptions<StripeSettings> stripeOptions,
            IPriceService pricing)
        {
            _context = context;
            _stripe = stripeOptions.Value; // Secret & Publishable from config/secrets
            _pricing = pricing;
        }

        /// <summary>
        /// GET: /ParkPasses/Index
        /// </summary>
        public IActionResult Index()
        {
            // Pull current seasonal prices from your pricing service
            var prices = _pricing.GetCurrentPrices();

            // Keep using your existing PassViewModel shape
            var vm = new PassViewModel
            {
                CurrentPrice = prices.Personal,  // backwards-compat (Personal as "current")
                PhaseLabel = prices.SeasonName,
                Blurb = "Same benefits across all passes — coverage size is the only difference."
            };

            // Also expose all three prices (so the view can render the table for Personal/Family/Family+)
            ViewBag.PersonalPrice = prices.Personal;
            ViewBag.FamilyPrice = prices.Family;
            ViewBag.FamilyPlusPrice = prices.FamilyPlus;

            return View(vm);
        }

        /// <summary>
        /// GET: /ParkPasses/Buy
        /// </summary>
        public IActionResult Buy()
        {
            var prices = _pricing.GetCurrentPrices();

            var vm = new PassBuyViewModel
            {
                PhaseLabel = prices.SeasonName,
                // "CurrentPrice" here is just a display fallback; actual charge is computed in CreatePaymentIntent
                CurrentPrice = prices.Personal,
                PublishableKey = _stripe.PublishableKey,
                PassTypes = new[] { "Personal", "Family", "Family+" }
            };

            return View(vm);
        }

        /// <summary>
        /// POST: /ParkPasses/CreatePaymentIntent  (called by fetch from the view)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePaymentIntent([FromBody] PassPurchaseRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.PassType))
                return BadRequest("Missing email or pass type.");

            // Load seasonal prices once, then pick the exact pass type price (no multipliers)
            var prices = _pricing.GetCurrentPrices();

            decimal selectedPrice = req.PassType switch
            {
                "Personal" => prices.Personal,
                "Family" => prices.Family,
                "Family+" => prices.FamilyPlus,
                "FamilyPlus" => prices.FamilyPlus, // accept both labels
                _ => prices.Personal
            };

            var amountCents = (long)Math.Round(selectedPrice * 100m);

            StripeConfiguration.ApiKey = _stripe.SecretKey;

            var options = new PaymentIntentCreateOptions
            {
                Amount = amountCents,
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
                    ["nnl_phase"] = prices.SeasonName
                }
            };

            var service = new PaymentIntentService();
            var intent = await service.CreateAsync(options);

            return Json(new { clientSecret = intent.ClientSecret });
        }

        // GET: /ParkPasses/Success
        public IActionResult Success() => View();

        // DTO used by CreatePaymentIntent
        public class PassPurchaseRequest
        {
            public string PassType { get; set; } = "";
            public string Email { get; set; } = "";
        }
    }
}
