using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NeverNeverLand.Services; // IEmailService
using Stripe;
using Stripe.Checkout;

namespace NeverNeverLand.Controllers
{
    [AllowAnonymous] // webhooks are unauthenticated
    public class StripeController : Controller
    {
        private readonly IEmailService _emailService;
        private readonly string _webhookSecret;

        public StripeController(IEmailService emailService, IConfiguration config)
        {
            _emailService = emailService;
            _webhookSecret = config["Stripe:WebhookSecret"] ?? ""; // set in appsettings.json
        }

        public IActionResult Index() => View();

        [HttpPost]
        [Route("stripe/webhook")]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> StripeWebhook()
        {
            // Read the raw body once (Stripe requires this exact payload for signature verification)
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

            try
            {
                var stripeEvent = EventUtility.ConstructEvent(
                    json,
                    Request.Headers["Stripe-Signature"],
                    _webhookSecret
                );

                if (stripeEvent.Type == "checkout.session.completed")
                {
                    var session = stripeEvent.Data.Object as Stripe.Checkout.Session;
                    var email = session?.CustomerDetails?.Email ?? session?.CustomerEmail;
                    if (!string.IsNullOrWhiteSpace(email))
                    {
                        await _emailService.SendTicketAsync(
                            email,
                            "Your Never Never Land Ticket",
                            "<p>Thank you for your purchase! Here is your ticket to Never Never Land.</p>"
                        );
                    }
                }

                // Acknowledge reception so Stripe doesn't retry
                return Ok();
            }
            catch (StripeException)
            {
                return BadRequest(); // invalid signature or other Stripe error
            }
        }
    }
}
