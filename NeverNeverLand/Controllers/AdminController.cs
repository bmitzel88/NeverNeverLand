using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NeverNeverLand.Data;
using NeverNeverLand.Models;
using NeverNeverLand.Models.ViewModels;
using System.Linq;
using System.Threading.Tasks;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _db;
    public AdminController(ApplicationDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var vm = new AdminPricingViewModel
        {
            SeasonOptions = await _db.Seasons
                .Where(s => s.IsActive)
                .OrderByDescending(s => s.AlwaysOn).ThenBy(s => s.StartDate)
                .Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name })
                .ToListAsync(),
            AdmissionTypeOptions = new List<SelectListItem>
            {
                new("Adult", "Adult"),
                new("Child", "Child"),
                new("Infant", "Infant")
            },
            Prices = await _db.Prices
                .Include(p => p.Season)
                .OrderByDescending(p => p.IsActive)
                .ThenBy(p => p.Season.Name)
                .ThenBy(p => p.Item)
                .ToListAsync()
        };
        if (vm.SeasonOptions.Count > 0)
            vm.SelectedSeasonId = int.Parse(vm.SeasonOptions[0].Value!);
        return View(vm);
    }

    [HttpPost]
    public async Task<IActionResult> CreatePrice(AdminPricingViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            vm.SeasonOptions = await _db.Seasons.Where(s => s.IsActive)
                .OrderByDescending(s => s.AlwaysOn).ThenBy(s => s.StartDate)
                .Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name })
                .ToListAsync();
            vm.AdmissionTypeOptions = new List<SelectListItem>
            {
                new("Adult","Adult"), new("Child","Child"), new("Infant","Infant")
            };
            vm.Prices = await _db.Prices
                .Include(p => p.Season)
                .OrderByDescending(p => p.IsActive)
                .ThenBy(p => p.Season.Name)
                .ThenBy(p => p.Item)
                .ToListAsync();
            return View("Index", vm);
        }
        var current = await _db.Prices
            .Where(p => p.SeasonId == vm.SelectedSeasonId
                     && p.Item == vm.SelectedAdmissionType
                     && p.IsActive)
            .ToListAsync();
        foreach (var p in current) p.IsActive = false;
        _db.Prices.Add(new Price
        {
            SeasonId = vm.SelectedSeasonId,
            Item = vm.SelectedAdmissionType,
            Amount = vm.Amount,
            Currency = vm.Currency,
            EffectiveStartUtc = DateTime.UtcNow,
            IsActive = true
        });
        _db.PriceChangeLogs.Add(new PriceChangeLog
        {
            SeasonId = vm.SelectedSeasonId,
            TicketId = 0,
            ChangedByUserId = User?.Identity?.Name ?? "admin",
            OldAmount = current.FirstOrDefault()?.Amount ?? 0m,
            NewAmount = vm.Amount,
            Currency = vm.Currency,
            Note = $"Set {vm.SelectedAdmissionType} price"
        });
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    // --- Event Management ---
    [HttpGet]
    public IActionResult Events()
    {
        // For demo: use in-memory list, replace with DB fetch for real events
        var events = new List<EventsViewModel>();
        return View(events);
    }
    [HttpGet]
    public IActionResult CreateEvent() => View();
    [HttpPost]
    public IActionResult CreateEvent(EventsViewModel model)
    {
        // TODO: Save event to DB
        return RedirectToAction("Events");
    }
    [HttpGet]
    public IActionResult EditEvent(string title)
    {
        // TODO: Fetch event by title
        return View();
    }
    [HttpPost]
    public IActionResult EditEvent(EventsViewModel model)
    {
        // TODO: Update event in DB
        return RedirectToAction("Events");
    }
    [HttpGet]
    public IActionResult DeleteEvent(string title)
    {
        // TODO: Delete event by title
        return RedirectToAction("Events");
    }

    // --- Ticket Prices ---
    [HttpGet]
    public async Task<IActionResult> TicketPrices()
    {
        var prices = await _db.Prices.Where(p => p.Kind == "Ticket").ToListAsync();
        return View(prices);
    }
    // --- Park Pass Prices ---
    [HttpGet]
    public async Task<IActionResult> PassPrices()
    {
        var prices = await _db.Prices.Where(p => p.Kind == "Pass").ToListAsync();
        return View(prices);
    }
    // --- Alerts ---
    [HttpGet]
    public IActionResult Alerts()
    {
        // For demo: use in-memory list, replace with DB fetch for real alerts
        var alerts = new List<string>();
        return View(alerts);
    }
    [HttpPost]
    public IActionResult Alerts(string alert)
    {
        // TODO: Save alert to DB
        return RedirectToAction("Alerts");
    }
}