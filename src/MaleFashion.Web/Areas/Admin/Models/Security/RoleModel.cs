using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MaleFashion.Web.Areas.Admin.Models.Security
{
    public class RoleOptionItem
    {
        public string Value { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class RoleModel
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Please select a role.")]
        [StringLength(100, ErrorMessage = "Role name cannot exceed 100 characters.")]
        [Display(Name = "Role Name")]
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;
        public bool IsSystemRole { get; set; }
        public int UsersCount { get; set; }

        public List<RoleOptionItem> AvailableRoles { get; set; } = new List<RoleOptionItem>();
    }
}

