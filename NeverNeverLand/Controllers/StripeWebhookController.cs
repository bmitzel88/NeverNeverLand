using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NeverNeverLand.Data;
using NeverNeverLand.Models;
using Stripe;
using System.Text;
using System.Text.Json;

namespace NeverNeverLand.Controllers
{
    [ApiController]
    [Route("api/stripe/webhook")]
    public class StripeWebhookController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<StripeWebhookController> _logger;
        private readonly IConfiguration _config;

        public StripeWebhookController(ApplicationDbContext db, ILogger<StripeWebhookController> logger, IConfiguration config)
        {
            _db = db;
            _logger = logger;
            _config = config;
        }

        [HttpPost]
        public async Task<IActionResult> Post()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            var endpointSecret = _config["Stripe:WebhookSecret"];
            Event stripeEvent;

            try
            {
                var signatureHeader = Request.Headers["Stripe-Signature"];
                stripeEvent = EventUtility.ConstructEvent(json, signatureHeader, endpointSecret);
            }
            catch (StripeException e)
            {
                _logger.LogError(e, "Stripe webhook signature verification failed.");
                return BadRequest();
            }

            if (stripeEvent.Type == EventTypes.CheckoutSessionCompleted)
                {
                var session = stripeEvent.Data.Object as Stripe.Checkout.Session;
                if (session != null)
                {
                    var userId = session.Metadata["userId"];
                    var itemType = session.Metadata["itemType"]; // "ticket" or "parkpass"
                    var quantity = int.TryParse(session.Metadata["quantity"], out var q) ? q : 1;
                    var passType = session.Metadata.ContainsKey("passType") ? session.Metadata["passType"] : "Personal";

                    if (itemType == "ticket")
                    {
                        for (int i = 0; i < quantity; i++)
                        {
                            var holderName = session.Metadata.ContainsKey("holderName") ? session.Metadata["holderName"] : (session.CustomerEmail ?? "Member");
                            var holderAge = session.Metadata.ContainsKey("holderAge") && int.TryParse(session.Metadata["holderAge"], out var age) ? age : 0;
                            var admissionType = session.Metadata.ContainsKey("admissionType") ? session.Metadata["admissionType"] : "Adult";

                            var ticket = new Ticket
                            {
                                UserId = userId,
                                HolderName = holderName,
                                HolderAge = holderAge,
                                AdmissionType = admissionType,
                                PricePaid = (decimal)(session.AmountTotal / 100.0),
                                Currency = session.Currency?.ToUpper() ?? "USD",
                                PurchaseDate = DateTime.UtcNow,
                                ExpirationDate = DateTime.UtcNow.AddMonths(6),
                                IsUsed = false
                            };
                            _db.Ticket.Add(ticket);
                        }
                        await _db.SaveChangesAsync();
                    }
                    else if (itemType == "parkpass")
                    {
                        var now = DateTime.UtcNow;
                        var pass = new ParkPass
                        {
                            UserId = userId, 
                            Type = passType,
                            SeasonYear = now.Year,
                            ExpiresAt = new DateTime(now.Year, 10, 31),
                            Status = "Active",
                            QrToken = Guid.NewGuid().ToString("N"),
                            MaxOwners = passType == "Family" || passType == "Family+" ? 2 : 1,
                            MaxGuests = passType == "Family+" ? 7 : passType == "Family" ? 4 : 2
                        };
                        _db.ParkPass.Add(pass);
                        await _db.SaveChangesAsync();
                    }
                }
            }

            return Ok();
        }
    }
}
