using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel.DataAnnotations;

namespace MaleFashion.Web.Areas.Admin.Models.CMS
{
    public class TestimonialModel
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Author Name is required.")]
        [StringLength(150, ErrorMessage = "Author Name cannot exceed 150 characters.")]
        [Display(Name = "Author Name")]
        public string AuthorName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Profession / Title is required.")]
        [StringLength(150, ErrorMessage = "Profession cannot exceed 150 characters.")]
        [Display(Name = "Profession / Title")]
        public string AuthorProfession { get; set; } = string.Empty;

        [Required(ErrorMessage = "Testimonial Quote is required.")]
        [StringLength(1000, ErrorMessage = "Quote cannot exceed 1000 characters.")]
        [DataType(DataType.MultilineText)]
        [Display(Name = "Testimonial Quote")]
        public string Quote { get; set; } = string.Empty;

        [Display(Name = "Author Avatar")]
        public IFormFile? Avatar { get; set; }

        public string? AvatarUrl { get; set; }
    }
}

