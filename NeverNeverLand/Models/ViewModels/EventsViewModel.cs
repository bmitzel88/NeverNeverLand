namespace NeverNeverLand.Models.ViewModels
{
    public record EventsViewModel
    {
        public string? ImageUrl { get; init; }
        public string Title { get; init; } = "";
        public DateTimeOffset StartLocal { get; init; }
        public DateTimeOffset? EndLocal { get; init; }
        public string? Location { get; init; }
        public string? Description { get; init; }
    }
}
