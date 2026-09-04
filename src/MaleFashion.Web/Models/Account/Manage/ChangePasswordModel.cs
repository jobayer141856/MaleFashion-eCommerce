using System.ComponentModel.DataAnnotations;

namespace MaleFashion.Web.Models.Account.Manage
{
    public class ChangePasswordModel
    {
        [Required]
        [DataType(DataType.Password)]
        public string CurrentPassword { get; set; } = default!;

        [Required]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; } = default!;

        [Required]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = default!;

    }
}
