using System;
using System.ComponentModel.DataAnnotations;

namespace MaleFashion.Web.Areas.Admin.Models.Support
{
    public class StoreLocationModel
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Location title is required.")]
        [StringLength(150, ErrorMessage = "Title cannot exceed 150 characters.")]
        [Display(Name = "Location Title / Country")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Physical address is required.")]
        [StringLength(500, ErrorMessage = "Address cannot exceed 500 characters.")]
        [Display(Name = "Address")]
        public string Address { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required.")]
        [StringLength(50, ErrorMessage = "Phone number cannot exceed 50 characters.")]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Invalid email address format.")]
        [StringLength(256, ErrorMessage = "Email cannot exceed 256 characters.")]
        [Display(Name = "Contact Email (Optional)")]
        public string? Email { get; set; }

        [Display(Name = "Display Order")]
        public int DisplayOrder { get; set; } = 0;

        [Display(Name = "Active Status")]
        public bool IsActive { get; set; } = true;
    }
}

