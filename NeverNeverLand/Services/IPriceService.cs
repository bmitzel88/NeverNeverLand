namespace NeverNeverLand.Services
{
    public record PassPrices(decimal Personal, decimal Family, decimal FamilyPlus, string SeasonName);

    public interface IPriceService
    {
        PassPrices GetCurrentPrices(DateTime? nowUtc = null);
    }
}
