using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel.DataAnnotations;

namespace MaleFashion.Web.Areas.Admin.Models.CMS
{
    public class TeamMemberModel
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Full Name is required.")]
        [StringLength(150, ErrorMessage = "Name cannot exceed 150 characters.")]
        [Display(Name = "Full Name")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Role/Designation is required.")]
        [StringLength(150, ErrorMessage = "Role cannot exceed 150 characters.")]
        [Display(Name = "Role / Designation")]
        public string Role { get; set; } = string.Empty;

        [Display(Name = "Photo")]
        public IFormFile? Photo { get; set; }

        public string? PhotoUrl { get; set; }
    }
}

