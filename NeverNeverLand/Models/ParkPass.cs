using System.ComponentModel.DataAnnotations.Schema;

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
        public required string Name { get; set; }
        /// <summary>
        /// Description of the park pass type
        /// </summary>
        public required string Description { get; set; }
        /// <summary>
        /// Price of the park pass
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
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
