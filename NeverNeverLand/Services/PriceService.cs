using Microsoft.Extensions.Configuration;

namespace NeverNeverLand.Services
{
    internal class SeasonConfig
    {
        public string Name { get; set; } = "";
        public string Start { get; set; } = ""; // "MM-dd"
        public string End { get; set; } = ""; // "MM-dd"
        public Dictionary<string, decimal> Prices { get; set; } = new();
    }

    public class PriceService : IPriceService
    {
        private readonly List<SeasonConfig> _seasons;
        private readonly TimeZoneInfo _tz;

        public PriceService(IConfiguration config)
        {
            var section = config.GetSection("Pricing");
            var tzId = section.GetValue<string>("TimeZone") ?? "America/Los_Angeles";
            _tz = TimeZoneInfo.FindSystemTimeZoneById(tzId);

            _seasons = section.GetSection("Seasons").Get<List<SeasonConfig>>() ?? new();
        }

        public PassPrices GetCurrentPrices(DateTime? nowUtc = null)
        {
            var nowLocal = TimeZoneInfo.ConvertTimeFromUtc(nowUtc ?? DateTime.UtcNow, _tz);
            var season = ResolveSeason(nowLocal);

            decimal Get(string key) => season.Prices.TryGetValue(key, out var v) ? v : 0m;

            return new PassPrices(
                Personal: Get("Personal"),
                Family: Get("Family"),
                FamilyPlus: Get("FamilyPlus"),
                SeasonName: season.Name
            );
        }

        private SeasonConfig ResolveSeason(DateTime localNow)
        {
            SeasonConfig? fallbackClosest = null;
            TimeSpan? bestDistance = null;

            foreach (var s in _seasons)
            {
                var (start, end) = BuildSeasonRange(localNow, s.Start, s.End);

                if (localNow.Date >= start.Date && localNow.Date <= end.Date)
                    return s;

                var distance = (localNow - start).Duration();
                if (bestDistance is null || distance < bestDistance.Value)
                {
                    bestDistance = distance;
                    fallbackClosest = s;
                }
            }

            return fallbackClosest ?? (_seasons.Count > 0 ? _seasons[0] : new SeasonConfig
            {
                Name = "Unknown",
                Start = "01-01",
                End = "12-31",
                Prices = new() { ["Personal"] = 0, ["Family"] = 0, ["FamilyPlus"] = 0 }
            });
        }

        private static (DateTime start, DateTime end) BuildSeasonRange(DateTime pivot, string startMd, string endMd)
        {
            var s = DateTime.ParseExact(startMd, "MM-dd", null);
            var e = DateTime.ParseExact(endMd, "MM-dd", null);

            var start = new DateTime(pivot.Year, s.Month, s.Day);
            var end = new DateTime(pivot.Year, e.Month, e.Day);

            if (end < start)
            {
                if (pivot.Month < start.Month)
                    start = new DateTime(pivot.Year - 1, s.Month, s.Day);
                else
                    end = new DateTime(pivot.Year + 1, e.Month, e.Day);
            }

            end = end.Date.AddDays(1).AddTicks(-1); // inclusive
            return (start, end);
        }
    }
}
