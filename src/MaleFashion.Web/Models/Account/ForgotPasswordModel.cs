using System.ComponentModel.DataAnnotations;

namespace MaleFashion.Web.Models.Account
{
    public class ForgotPasswordModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = default!;
    }
}
