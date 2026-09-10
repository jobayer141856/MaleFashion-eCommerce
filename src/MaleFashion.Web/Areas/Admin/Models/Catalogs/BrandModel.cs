using System.ComponentModel.DataAnnotations;

namespace MaleFashion.Web.Areas.Admin.Models.Catalogs
{
    public class BrandModel
    {
        public Guid Id { get; set; }
        [Required(ErrorMessage = "Brand name is required.")]
        [StringLength(150, ErrorMessage = "Brand name cannot exceed 150 characters.")]
        public string Name { get; set; } = string.Empty;
        [Display(Name = "Brand Logo")]
        public IFormFile? Logo { get; set; }
        public string? LogoUrl { get; set; }
        [Display(Name = "Active Status")]
        public bool IsActive { get; set; } = true;
    }
}
