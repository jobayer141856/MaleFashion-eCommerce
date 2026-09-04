using System.ComponentModel.DataAnnotations;

namespace MaleFashion.Web.Models.Account.Manage
{
    public class TwoFactorAuthenticationModel
    {
        public bool Is2faEnabled { get; set; }

        public bool HasAuthenticator { get; set; }

        public string? SharedKey { get; set; }

        public string? AuthenticatorUri { get; set; }

        public int RecoveryCodesLeft { get; set; }

        [Required(ErrorMessage = "Please enter the 6-digit verification code.")]
        [StringLength(7, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Text)]
        [Display(Name = "Verification Code")]
        public string? Code { get; set; }
    }
}

