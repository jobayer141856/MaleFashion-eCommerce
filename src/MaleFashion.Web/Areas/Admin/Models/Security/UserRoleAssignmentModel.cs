using System;
using System.Collections.Generic;

namespace MaleFashion.Web.Areas.Admin.Models.Security
{
    public class RoleCheckboxItem
    {
        public Guid RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public bool IsSelected { get; set; }
        public bool IsDisabled { get; set; }
    }

    public class UserRoleAssignmentModel
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public List<RoleCheckboxItem> Roles { get; set; } = new List<RoleCheckboxItem>();
    }
}

