namespace NeverNeverLand.Models
{
    public class ParkPass
    {
        /// <summary>
        /// Unique identifier for the park pass
        /// </summary>
        public int Id { get; set; } 
        /// <summary>
        /// Name/Type of the park pass
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Description of the park pass type
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// Price of the park pass
        /// </summary>
        public decimal Price { get; set; }
        /// <summary>
        /// Start date of validity
        /// </summary>
        public DateTime ValidFrom { get; set; }
        /// <summary>
        /// End date of validity
        /// </summary>
        public DateTime ValidUntil { get; set; }
        /// <summary>
        /// Indicates if the park pass is currently active
        /// </summary>
        public bool IsActive { get; set; } 
    }
}
