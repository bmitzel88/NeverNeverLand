using System.Diagnostics;
using System.Net.NetworkInformation;
using Microsoft.AspNetCore.Mvc;
using NeverNeverLand.Models.ViewModels;

namespace NeverNeverLand.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }
        
        public IActionResult Attractions()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Contact() {
            return View(new ContactViewModel());
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public IActionResult Contact(ContactViewModel vm)
        {
            // Basic honeypot check
            if (Request.Form.TryGetValue("Website", out var hp) && !string.IsNullOrWhiteSpace(hp))
            {
                // silently ignore spam
                TempData["ContactSuccess"] = "Thanks!";
                return RedirectToAction(nameof(Contact));
            }

            if (!ModelState.IsValid) return View(vm);

            // Compose message
            var body =
            $@"Contact Form
            Name: {vm.Name}
            Email: {vm.Email}
            Subject: {vm.Subject}

            {vm.Message}";

            // TODO: plug in IEmailService (SendGrid)
            // await _email.SendAsync("support@neverneverland.com", $"Contact: {vm.Subject}", body);

            TempData["ContactSuccess"] = "Thanks—your message has been sent!";
            return RedirectToAction(nameof(Contact));
        }

        [HttpGet]
        public IActionResult Events()
        {
            // TODO: Replace with DB/IEventsService; this is sample data.
            var list = new List<EventsViewModel>
        {
            new() {
                Title = "Test Event",
                StartLocal = DateTimeOffset.Now.AddDays(21).AddHours(10),
                EndLocal   = DateTimeOffset.Now.AddDays(21).AddHours(16),
                Location = "Main Meadow",
                Description = "Soft opening with guided walk-throughs and photo ops."
            },
            new() {
                Title = "Storytime in the Grove",
                StartLocal = DateTimeOffset.Now.AddDays(35).AddHours(11),
                Location = "Fairy Grove",
                Description = "Interactive reading hour; free with park admission."
            }
        };

            return View(list);
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Terms()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
