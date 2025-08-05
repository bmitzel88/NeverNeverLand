using Microsoft.AspNetCore.Mvc;
using NeverNeverLand.Models;
using NeverNeverLand.Services;
using Stripe;
using Stripe.Checkout;

namespace NeverNeverLand.Controllers
{
    public class StripeController : Controller
    {
        private readonly IEmailService _emailService;
        
        public StripeController(IEmailService emailService)
        {
            _emailService = emailService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [Route("stripe/webhook")]
        public async Task<IActionResult> StripeWebhook()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            var secret = ""; 


            try
            {
                var stripeEvent = EventUtility.ConstructEvent(
                    json,
                    Request.Headers["Stripe-Signature"],
                    secret
                );

                if (stripeEvent.Type == "checkout.session.completed")
                {
                    var session = stripeEvent.Data.Object as Session;

                    // Extract email
                    var email = session.CustomerDetails.Email;

                    // Send email with ticket (call helper method)
                    await _emailService.SendTicketAsync(
                        email,
                        "Your Never Never Land Ticket",
                        "<p>Thank you for your purchase! Here is your ticket to Never Never Land.</p>"
                    );
                }

                return Ok();
            }
            catch (StripeException e)
            {
                return BadRequest();
            }
        }

    }
}
