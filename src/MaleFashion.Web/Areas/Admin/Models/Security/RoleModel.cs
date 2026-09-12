using System;
using System.ComponentModel.DataAnnotations;

namespace MaleFashion.Web.Areas.Admin.Models.Security
{
    public class RoleModel
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Role name is required.")]
        [StringLength(100, ErrorMessage = "Role name cannot exceed 100 characters.")]
        [RegularExpression(@"^[a-zA-Z0-9_\-\s]+$", ErrorMessage = "Role name can only contain letters, numbers, hyphens, and underscores.")]
        [Display(Name = "Role Name")]
        public string Name { get; set; } = string.Empty;

        public bool IsSystemRole { get; set; }
        public int UsersCount { get; set; }
    }
}

