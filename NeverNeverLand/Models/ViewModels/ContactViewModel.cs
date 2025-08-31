using System.ComponentModel.DataAnnotations;

namespace NeverNeverLand.Models.ViewModels
{
    public class ContactViewModel
    {
        [Required, StringLength(80)]
        public string Name { get; set; } = "";

        [Required, EmailAddress]
        public string Email { get; set; } = "";

        [Required, StringLength(120)]
        public string Subject { get; set; } = "";

        [Required, StringLength(4000)]
        public string Message { get; set; } = "";

        // Honeypot (not bound to view directly)
        // public string? Website { get; set; }

        [Required(ErrorMessage = "Please agree to the Privacy Policy.")]
        public bool AgreeToPrivacy { get; set; }
    }
}
