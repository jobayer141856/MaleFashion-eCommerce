using System.ComponentModel.DataAnnotations;

namespace MaleFashion.Web.Models.Support
{
    public class NewsletterSubscribeModel
    {
        [Required(ErrorMessage = "Please enter your email address.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        [StringLength(256, ErrorMessage = "Email cannot exceed 256 characters.")]
        public string Email { get; set; } = string.Empty;
    }
}

