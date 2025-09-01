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

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Attractions(string? id)
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

        [HttpGet]
        public IActionResult About()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Terms()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Privacy()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Visit()
        {
            var vm = new VisitViewModel
            {
                TodayHoursText = "10:00 AM – 6:00 PM",
                IsOpen = true,
                StatusUpdatedText = "Updated 12:05 PM",
                EventsTodayCount = 3,
                TicketPriceFrom = 19.00m,  // you can replace with your pricing service
                PassPriceFrom = 69.00m,
                ParkAddress = "123 Evergreen Way, Tacoma, WA 98407",
                TransitRoute = "10/11",
                UpcomingEvents = new[]
                {
            new EventSummary { Id="storytime-3p", Title="Live Storytime: Three Little Pigs", Location="Story Stage", WhenLocalText="Today · 3:00 PM", Summary="Interactive show for all ages." },
            new EventSummary { Id="meetgreet-fri", Title="Character Meet & Greet", Location="Fairytale Gate", WhenLocalText="Fri · 3:00 PM", Summary="Photos and autographs with our cast." },
            new EventSummary { Id="puppet-sat", Title="Puppet Workshop", Location="Castle Fort", WhenLocalText="Sat · 1:30 PM", Summary="Make-and-take craft for kids." }
        }
            };

            return View(vm);
        }

        [HttpGet]
        public IActionResult Hours()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Accessibility()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Directions()
        {
            return View();
        }

        [HttpGet]
        public IActionResult FAQ()
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
