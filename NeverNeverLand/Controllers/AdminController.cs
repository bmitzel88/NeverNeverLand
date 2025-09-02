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
    public async Task<IActionResult> Index(
    int? seasonId, string kind = "All", string? item = null,
    bool includeInactive = false, string? q = null)
    {
        // Build season dropdown with dates
        var seasons = await _db.Seasons
            .Where(s => s.IsActive)
            .OrderBy(s => s.StartDate)
            .Select(s => new SelectListItem
            {
                Value = s.Id.ToString(),
                Text = $"{s.Name} ({s.StartDate:MMM d} – {s.EndDate:MMM d})"
            })
            .ToListAsync();

        // Admission item options depend on kind (or show all when "All")
        static List<SelectListItem> ItemsForKind(string k) =>
            k == "Pass"
                ? new() { new("Personal", "Personal"), new("Family", "Family"), new("Family+", "Family+") }
            : k == "Ticket"
                ? new() { new("Adult", "Adult"), new("Child", "Child"), new("Infant", "Infant") }
                : new() {
                new("Adult","Adult"), new("Child","Child"), new("Infant","Infant"),
                new("Personal","Personal"), new("Family","Family"), new("Family+","Family+")
                };

        var query = _db.Prices
            .Include(p => p.Season)
            .AsQueryable();

        if (!includeInactive)
            query = query.Where(p => p.IsActive);

        if (seasonId.HasValue)
            query = query.Where(p => p.SeasonId == seasonId.Value);

        if (kind == "Ticket" || kind == "Pass")
            query = query.Where(p => p.Kind == kind);

        if (!string.IsNullOrWhiteSpace(item))
            query = query.Where(p => p.Item == item);

        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(p =>
                p.Item.Contains(q) ||
                p.Season.Name.Contains(q));

        // Order by Season -> Kind -> Item
        var prices = await query
            .OrderBy(p => p.Season.StartDate)
            .ThenBy(p => p.Kind)
            .ThenBy(p => p.Item)
            .ToListAsync();

        var vm = new AdminPricingViewModel
        {
            SeasonOptions = seasons,
            KindOptions = new() { new("Ticket", "Ticket"), new("Pass", "Pass") },
            AdmissionTypeOptions = ItemsForKind(kind),
            Prices = prices,

            // Filters reflect query
            FilterSeasonId = seasonId,
            FilterKind = kind,
            FilterItem = item,
            IncludeInactive = includeInactive,
            Q = q
        };

        // Default selection for create form
        vm.SelectedSeasonId = seasonId ?? (seasons.Count > 0 ? int.Parse(seasons[0].Value!) : 0);
        vm.SelectedKind = "Ticket";
        vm.AdmissionTypeOptions = ItemsForKind(vm.SelectedKind);

        return View(vm);
    }

    [HttpPost]
    public async Task<IActionResult> CreatePrice(AdminPricingViewModel vm)
    {
        // repopulate option lists for redisplay if validation fails
        List<SelectListItem> ItemsForKind(string k) =>
            k == "Pass"
                ? new() { new("Personal", "Personal"), new("Family", "Family"), new("Family+", "Family+") }
            : new() { new("Adult", "Adult"), new("Child", "Child"), new("Infant", "Infant") };

        if (!ModelState.IsValid)
        {
            vm.SeasonOptions = await _db.Seasons.Where(s => s.IsActive)
                .OrderBy(s => s.StartDate)
                .Select(s => new SelectListItem { Value = s.Id.ToString(), Text = $"{s.Name} ({s.StartDate:MMM d} – {s.EndDate:MMM d})" })
                .ToListAsync();
            vm.KindOptions = new() { new("Ticket", "Ticket"), new("Pass", "Pass") };
            vm.AdmissionTypeOptions = ItemsForKind(vm.SelectedKind);
            vm.Prices = await _db.Prices.Include(p => p.Season).Where(p => p.IsActive)
                .OrderBy(p => p.Season.StartDate).ThenBy(p => p.Kind).ThenBy(p => p.Item).ToListAsync();
            return View("Index", vm);
        }

        // Deactivate current active price for this (Season, Kind, Item)
        var current = await _db.Prices
            .Where(p => p.SeasonId == vm.SelectedSeasonId
                        && p.Kind == vm.SelectedKind
                        && p.Item == vm.SelectedAdmissionType
                        && p.IsActive)
            .ToListAsync();

        foreach (var p in current)
        {
            p.IsActive = false;
            p.EffectiveEndUtc = DateTime.UtcNow;
        }

        // Create new active price
        _db.Prices.Add(new Price
        {
            SeasonId = vm.SelectedSeasonId,
            Kind = vm.SelectedKind,               // "Ticket" or "Pass"
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
            Note = $"Set {vm.SelectedKind}:{vm.SelectedAdmissionType} price"
        });

        await _db.SaveChangesAsync();

        // Redirect back to same season & kind context
        return RedirectToAction(nameof(Index), new
        {
            seasonId = vm.SelectedSeasonId,
            kind = vm.SelectedKind
        });
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