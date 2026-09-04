using System.ComponentModel.DataAnnotations;

namespace MaleFashion.Web.Models.Account
{
    public class LoginWithRecoveryCodeModel
    {
        public string? ReturnUrl { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "Recovery Code")]
        public string RecoveryCode { get; set; } = default!;
    }
}
