using MaleFashion.Domain.Contracts;
using System.ComponentModel.DataAnnotations;

namespace MaleFashion.Web.Areas.Admin.Models
{
    public class ProductModel
    {
        public Guid Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; }

        [Range(0.01, 999999)]
        public double Price { get; set; }

        [Display(Name="Picture")]
        public IFormFile? Image { get; set; }
        public string? ImageName { get; set; }
    }
}
