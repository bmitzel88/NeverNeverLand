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

        // List page
        public IActionResult Attractions() => View();

        // Detail page
        [HttpGet]
        public IActionResult Attraction(string id)
        {
            var map = GetAttractions();
            if (string.IsNullOrWhiteSpace(id)) return RedirectToAction(nameof(Attractions));

            id = id.ToLowerInvariant();
            if (!map.TryGetValue(id, out var vm)) return NotFound();

            return View(vm);
        }

        // In-memory content for the 4 opening locations
        private static Dictionary<string, AttractionDetailViewModel> GetAttractions()
        {
            return new Dictionary<string, AttractionDetailViewModel>(StringComparer.OrdinalIgnoreCase)
            {
                ["stage"] = new AttractionDetailViewModel
                {
                    Slug = "stage",
                    Title = "Stage",
                    Lead = "Live storytime, character shows, and performances.",
                    Description = "Our outdoor forest stage hosts short, family-friendly shows throughout the day. Seating is bench-style; feel free to come and go between scenes.",
                    LocationNote = "Near the main entrance.",
                    AccessibilityNote = "Bench seating with space for mobility devices at front rows. Music plays during shows.",
                    Highlights = new()
                {
                    "Storytime and sing-alongs",
                    "Character meet & waves after select shows",
                    "Shade from surrounding evergreens"
                },
                    ImageUrls = new()
                {
                    "/images/stage.png",
                    "/images/attractions/stage-2.jpg"
                }
                },
                ["the-shoe"] = new AttractionDetailViewModel
                {
                    Slug = "the-shoe",
                    Title = "The Shoe",
                    Lead = "Walk-through scene from “Old Woman Who Lived in a Shoe.”",
                    Description = "Step up and peek inside the giant hillside shoe. It’s a playful photo spot and a quick slide for little ones.",
                    LocationNote = "Along the central path.",
                    AccessibilityNote = "Stairs and a short slide; scene viewable from ground level.",
                    Highlights = new()
                {
                    "Classic nursery-rhyme vignette",
                    "Quick kid-friendly slide",
                    "Great photo backdrop"
                },
                    ImageUrls = new()
                {
                    "/images/shoe.png",
                    "/images/attractions/shoe-2.jpg"
                }
                },
                ["crooked-old-man"] = new AttractionDetailViewModel
                {
                    Slug = "crooked-old-man",
                    Title = "The Crooked Old Man’s House",
                    Lead = "Peek inside the crooked little house from the classic rhyme.",
                    Description = "A whimsical, off-kilter cottage with a small slide on the side. Notice the quirky angles and crooked trim!",
                    LocationNote = "Forest loop, just past the shoe.",
                    AccessibilityNote = "Stairs to slide; scene viewable from path.",
                    Highlights = new()
                {
                    "Crooked cottage details",
                    "Small slide for kids",
                    "Shaded seating nearby"
                },
                    ImageUrls = new()
                {
                    "/images/crooked.png",
                    "/images/attractions/crooked-2.jpg"
                }
                },
                ["storybook-trail"] = new AttractionDetailViewModel
                {
                    Slug = "storybook-trail",
                    Title = "Storybook Trail",
                    Lead = "Forest path featuring the rest of our nursery rhymes.",
                    Description = "Follow the gentle path through the trees to discover classic rhymes brought to life in small scenes and plaques.",
                    LocationNote = "Begins near the Stage and loops through the woods.",
                    AccessibilityNote = "Packed-gravel path with gentle grades. Benches along the route.",
                    Highlights = new()
                {
                    "Dozens of rhyme moments",
                    "Quiet nature walk",
                    "Benches and photo stops"
                },
                    ImageUrls = new()
                {
                    "/images/forest-trail.png",
                    "/images/attractions/trail-2.jpg",
                    "/images/attractions/trail-3.jpg"
                }
                }
            };
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
