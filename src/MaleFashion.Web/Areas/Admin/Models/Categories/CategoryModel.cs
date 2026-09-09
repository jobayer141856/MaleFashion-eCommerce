using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace MaleFashion.Web.Areas.Admin.Models.Categories
{
    public class CategoryModel
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Category name is required.")]
        [StringLength(150, ErrorMessage = "Name cannot exceed 150 characters.")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Parent Category")]
        public Guid? ParentCategoryId { get; set; }

        [Display(Name = "Active Status")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Category Image")]
        public IFormFile? Image { get; set; }
        public string? ImageName { get; set; }
        public IEnumerable<SelectListItem>? ParentCategories { get; set; }
    }
}
