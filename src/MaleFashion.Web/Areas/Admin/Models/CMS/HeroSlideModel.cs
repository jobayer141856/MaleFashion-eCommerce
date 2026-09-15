using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel.DataAnnotations;

namespace MaleFashion.Web.Areas.Admin.Models.CMS
{
    public class HeroSlideModel
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Subtitle / Tag is required.")]
        [StringLength(150, ErrorMessage = "Subtitle cannot exceed 150 characters.")]
        [Display(Name = "Subtitle / Tagline (e.g. Summer Collection)")]
        public string Subtitle { get; set; } = string.Empty;

        [Required(ErrorMessage = "Headline / Title is required.")]
        [StringLength(250, ErrorMessage = "Title cannot exceed 250 characters.")]
        [Display(Name = "Headline Title")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required.")]
        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters.")]
        [Display(Name = "Description")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Button text is required.")]
        [StringLength(50, ErrorMessage = "Button text cannot exceed 50 characters.")]
        [Display(Name = "Button Text (e.g. Shop now)")]
        public string ButtonText { get; set; } = "Shop now";

        [Required(ErrorMessage = "Target URL is required.")]
        [StringLength(500, ErrorMessage = "Target URL cannot exceed 500 characters.")]
        [Display(Name = "Target URL (e.g. /Shop)")]
        public string TargetUrl { get; set; } = "/Shop";

        [Display(Name = "Display Order")]
        public int DisplayOrder { get; set; } = 0;

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Hero Background Image")]
        public IFormFile? Photo { get; set; }

        public string? ImageUrl { get; set; }
    }
}

