using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NeverNeverLand.Data;
using NeverNeverLand.Models;
using NeverNeverLand.Models.ViewModels;
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
            _stripe = stripeOptions.Value;
            _pricing = pricing;
        }

        public IActionResult Index()
        {
            var seasonName = _pricing.GetCurrentSeasonName();
            var personalPrice = _pricing.GetCurrentPrices("Personal");
            var familyPrice = _pricing.GetCurrentPrices("Family");
            var familyPlusPrice = _pricing.GetCurrentPrices("Family+");

            var vm = new PassViewModel
            {
                PhaseLabel = seasonName,
                Blurb = "Same benefits across all passes — coverage size is the only difference.",
                CurrentPrice = personalPrice,
                PersonalPrice = personalPrice,
                FamilyPrice = familyPrice,
                FamilyPlusPrice = familyPlusPrice
            };
            return View(vm);
        }

        public IActionResult Buy(string type = "Personal")
        {
            var seasonName = _pricing.GetCurrentSeasonName();
            var personalPrice = _pricing.GetCurrentPrices("Personal");
            var familyPrice = _pricing.GetCurrentPrices("Family");
            var familyPlusPrice = _pricing.GetCurrentPrices("Family+");
            var normalized = NormalizePassType(type) ?? "Personal";

            var selectedPrice = normalized switch
            {
                "Family" => familyPrice,
                "Family+" => familyPlusPrice,
                _ => personalPrice
            };

            var vm = new PassBuyViewModel
            {
                PhaseLabel = seasonName,
                PublishableKey = _stripe.PublishableKey,
                SelectedPassType = normalized,
                CurrentPrice = selectedPrice,
                PersonalPrice = personalPrice,
                FamilyPrice = familyPrice,
                FamilyPlusPrice = familyPlusPrice,
                PassTypes = new[] { "Personal", "Family", "Family+" }
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePaymentIntent([FromBody] PassPurchaseRequest req)
        {
            if (string.IsNullOrWhiteSpace(req?.Email) || string.IsNullOrWhiteSpace(req?.PassType))
                return BadRequest("Missing email or pass type.");

            var type = NormalizePassType(req.PassType);
            if (type is null) return BadRequest("Unknown pass type.");

            var seasonName = _pricing.GetCurrentSeasonName();
            var personalPrice = _pricing.GetCurrentPrices("Personal");
            var familyPrice = _pricing.GetCurrentPrices("Family");
            var familyPlusPrice = _pricing.GetCurrentPrices("Family+");
            decimal selectedPrice = type switch
            {
                "Personal" => personalPrice,
                "Family" => familyPrice,
                "Family+" => familyPlusPrice,
                _ => personalPrice
            };
            if (selectedPrice <= 0) return BadRequest("Price not available.");

            var policy = GetPolicyFor(type);

            StripeConfiguration.ApiKey = _stripe.SecretKey;
            var amountCents = (long)Math.Round(selectedPrice * 100m, MidpointRounding.AwayFromZero);

            var options = new PaymentIntentCreateOptions
            {
                Amount = amountCents,
                Currency = "usd",
                AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions { Enabled = true },
                ReceiptEmail = req.Email,
                Description = $"{type} Season Pass ({seasonName})",
                Metadata = new Dictionary<string, string>
                {
                    ["nnl_item"] = "SeasonPass",
                    ["nnl_pass_type"] = type,
                    ["nnl_season"] = seasonName,
                    ["nnl_price_usd"] = selectedPrice.ToString("0.00"),
                    ["nnl_policy_maxOwners"] = policy.MaxOwners.ToString(),
                    ["nnl_policy_maxGuests"] = policy.MaxGuests.ToString()
                }
            };

            var service = new PaymentIntentService();
            var intent = await service.CreateAsync(options);

            return Json(new { clientSecret = intent.ClientSecret });
        }

        private static string? NormalizePassType(string t)
        {
            var s = (t ?? "").Trim().ToLowerInvariant();
            return s switch
            {
                "personal" => "Personal",
                "family" => "Family",
                "familyplus" => "Family+",
                "family+" => "Family+",
                _ => null
            };
        }

        // Personal: 1 adult owner + 2 guests
        // Family:   2 named adults + 4 guests
        // Family+:  2 named adults + 7 guests
        private static (int MaxOwners, int MaxGuests) GetPolicyFor(string type) =>
            type switch
            {
                "Personal" => (1, 2),
                "Family" => (2, 4),
                "Family+" => (2, 7),
                _ => (1, 2)
            };

        public async Task<IActionResult> Success()
        {
            // Save a new ParkPass for the logged-in user after successful payment
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
            if (user == null)
                return Challenge();

            // For demo: create a Personal pass for the current season. In production, use session/payment data for type, season, etc.
            var now = DateTime.UtcNow;
            var pass = new ParkPass
            {
                UserId = user.Id,
                Type = "Personal", // Set from purchase
                SeasonYear = now.Year,
                ExpiresAt = new DateTime(now.Year, 10, 31), // End of season
                Status = "Active",
                QrToken = Guid.NewGuid().ToString("N"),
                MaxOwners = 1,
                MaxGuests = 2
                
            };

            _context.ParkPass.Add(pass);
            await _context.SaveChangesAsync();

            return View();
        }

        public async Task<IActionResult> Manage()
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
            if (user == null) return Challenge();
            var pass = await _context.ParkPass
                .Where(p => p.UserId == user.Id && p.Status == "Active")
                .OrderByDescending(p => p.ExpiresAt)
                .FirstOrDefaultAsync();
            if (pass == null) return NotFound();
            return View(pass);
        }

        public class PassPurchaseRequest
        {
            public string PassType { get; set; } = "";
            public string Email { get; set; } = "";
        }
    }
}
