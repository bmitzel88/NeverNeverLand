namespace NeverNeverLand.Services
{
    public interface IPriceService
    {
        decimal GetCurrentPrices(string item, string channel = "Online", DateTime? nowUtc = null);
        string GetCurrentSeasonName(DateTime? nowUtc = null);
    }
}
