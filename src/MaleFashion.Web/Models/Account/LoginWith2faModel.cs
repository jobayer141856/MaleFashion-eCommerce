using System.ComponentModel.DataAnnotations;

namespace MaleFashion.Web.Models.Account
{
    public class LoginWith2faModel
    {
        [Required(ErrorMessage = "The 2FA code is required.")]
        [StringLength(7, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Text)]
        [Display(Name = "Authenticator Code")]
        public string TwoFactorCode { get; set; } = string.Empty;

        [Display(Name = "Remember this machine")]
        public bool RememberMachine { get; set; }

        public bool RememberMe { get; set; }

        public string? ReturnUrl { get; set; }
    }
}

