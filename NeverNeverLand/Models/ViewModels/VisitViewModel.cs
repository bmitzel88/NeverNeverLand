namespace NeverNeverLand.Models.ViewModels
{
    public class VisitViewModel
    {
        public string? TodayHoursText { get; set; }
        public bool IsOpen { get; set; }
        public string? StatusUpdatedText { get; set; }
        public int EventsTodayCount { get; set; }

        public decimal? TicketPriceFrom { get; set; }
        public decimal? PassPriceFrom { get; set; }

        public string? ParkAddress { get; set; }
        public string? TransitRoute { get; set; }

        public IEnumerable<EventSummary>? UpcomingEvents { get; set; }
    }

    public class EventSummary
    {
        public string Id { get; set; } = "";
        public string Title { get; set; } = "";
        public string Location { get; set; } = "";
        public string WhenLocalText { get; set; } = "";
        public string? Summary { get; set; }
    }
}
