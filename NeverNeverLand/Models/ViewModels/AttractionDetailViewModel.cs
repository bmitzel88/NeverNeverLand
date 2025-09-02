namespace NeverNeverLand.Models.ViewModels
{
    public class AttractionDetailViewModel
    {
        public string Slug { get; set; } = "";
        public string Title { get; set; } = "";
        public string Lead { get; set; } = "";
        public string Description { get; set; } = "";
        public string? LocationNote { get; set; }
        public string? AccessibilityNote { get; set; }
        public List<string> Highlights { get; set; } = new();
        public List<string> ImageUrls { get; set; } = new();
    }
}
