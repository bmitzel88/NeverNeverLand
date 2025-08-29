namespace NeverNeverLand.Models
{
    public class Season
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public bool AlwaysOn { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
