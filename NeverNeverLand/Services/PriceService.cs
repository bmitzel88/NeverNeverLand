using Microsoft.Extensions.Configuration;

namespace NeverNeverLand.Services
{
    internal class SeasonConfig
    {
        public string Name { get; set; } = "";
        public string Start { get; set; } = "";
        public string End { get; set; } = "";
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
            var season = _seasons.FirstOrDefault(s => IsWithinSeason(s, nowLocal))
                         ?? _seasons.First();

            return new PassPrices(
                season.Prices["Personal"],
                season.Prices["Family"],
                season.Prices["FamilyPlus"],
                season.Name
            );
        }

        private static bool IsWithinSeason(SeasonConfig s, DateTime now)
        {
            var start = DateTime.ParseExact(s.Start, "MM-dd", null).AddYears(now.Year - 1);
            var end = DateTime.ParseExact(s.End, "MM-dd", null).AddYears(now.Year - 1);

            if (end < start) // wraps new year
                end = end.AddYears(1);

            var checkDate = now.Date;
            return checkDate >= start.Date && checkDate <= end.Date;
        }
    }
}
