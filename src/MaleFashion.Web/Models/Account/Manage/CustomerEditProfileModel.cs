using System;
using System.ComponentModel.DataAnnotations;

namespace MaleFashion.Web.Models.Account.Manage
{
    public class CustomerEditProfileModel
    {
        [Required(ErrorMessage = "First name is required.")]
        [Display(Name = "First Name")]
        [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters.")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required.")]
        [Display(Name = "Last Name")]
        [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters.")]
        public string LastName { get; set; } = string.Empty;

        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required.")]
        [Display(Name = "Phone Number")]
        [Phone(ErrorMessage = "Please enter a valid phone number.")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Display(Name = "Date of Birth")]
        [DataType(DataType.Date)]
        public DateOnly? DateOfBirth { get; set; }

        [Display(Name = "Gender")]
        public string? Gender { get; set; }

        public bool IsEmailConfirmed { get; set; }
    }
}

