using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NeverNeverLand.Data;

namespace NeverNeverLand.Services
{
    public class PriceService : IPriceService
    {
        private readonly ApplicationDbContext _db;
        private readonly TimeZoneInfo _tz;

        public PriceService(ApplicationDbContext db, IConfiguration config)
        {
            _db = db;
            var tzId = config.GetSection("Pricing").GetValue<string>("TimeZone") ?? "America/Los_Angeles";
            _tz = TimeZoneInfo.FindSystemTimeZoneById(tzId);
        }

        public string GetCurrentSeasonName(DateTime? nowUtc = null)
        {
            var season = ResolveSeason(nowUtc);
            return season.Name;
        }

        public decimal GetCurrentPrices(string item, string channel = "Online", DateTime? nowUtc = null)
        {
            var season = ResolveSeason(nowUtc);

            // try exact item first (e.g., "Family+")
            var amount = _db.Prices.AsNoTracking()
                .Where(p => p.SeasonId == season.Id && p.Kind == "Pass" && p.Item == item
                            && p.Channel == channel && p.IsActive)
                .OrderByDescending(p => p.EffectiveStartUtc)
                .Select(p => p.Amount)
                .FirstOrDefault();

            if (amount > 0m) return amount;

            // fallback if DB used "FamilyPlus" instead of "Family+"
            if (string.Equals(item, "Family+", StringComparison.OrdinalIgnoreCase))
            {
                amount = _db.Prices.AsNoTracking()
                    .Where(p => p.SeasonId == season.Id && p.Kind == "Pass" && p.Item == "FamilyPlus"
                                && p.Channel == channel && p.IsActive)
                    .OrderByDescending(p => p.EffectiveStartUtc)
                    .Select(p => p.Amount)
                    .FirstOrDefault();
            }

            if (amount <= 0m)
                throw new InvalidOperationException($"No active price for pass '{item}' ({channel}) in season '{season.Name}'.");

            return amount;
        }

        private Models.Season ResolveSeason(DateTime? nowUtc)
        {
            var localNow = TimeZoneInfo.ConvertTimeFromUtc(nowUtc ?? DateTime.UtcNow, _tz);
            var today = DateOnly.FromDateTime(localNow);

            var season = _db.Seasons.AsNoTracking()
                .Where(s => s.IsActive &&
                            (s.AlwaysOn ||
                             (s.StartDate <= today && (s.EndDate == null || s.EndDate >= today))))
                .OrderByDescending(s => !s.AlwaysOn) // prefer dated over AlwaysOn
                .ThenBy(s => s.StartDate)
                .FirstOrDefault();

            if (season == null)
                throw new InvalidOperationException("No active season configured.");

            return season;
        }
    }
}
