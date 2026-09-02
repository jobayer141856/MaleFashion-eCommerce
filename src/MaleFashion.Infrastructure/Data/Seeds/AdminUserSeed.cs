using MaleFashion.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace MaleFashion.Infrastructure.Data.Seeds
{
    internal class AdminUserSeed
    {
        // Fixed GUIDs so EF Core knows the exact identity keys
        public static readonly Guid AdminUserId = new Guid("11111111-1111-1111-1111-111111111111");
        public static readonly Guid AdminRoleId = new Guid("00000000-0000-0000-0000-000000000001"); // Matches Admin ID from RoleSeeds
        public static ApplicationUser GetAdminUser()
        {
            var adminUser = new ApplicationUser
            {
                Id = AdminUserId,
                UserName = "admin@malefashion.com",
                NormalizedUserName = "ADMIN@MALEFASHION.COM",
                Email = "admin@malefashion.com",
                NormalizedEmail = "ADMIN@MALEFASHION.COM",
                EmailConfirmed = false,
                FirstName = "Admin",
                LastName = "User",
                SecurityStamp = "A7C934E2-55B1-4A89-B817-2D9981E3F1A0",
                ConcurrencyStamp = "F1E852D4-90C3-481B-8526-7B1A45C9E231",
                LockoutEnabled = false,
                AccessFailedCount = 0
            };
            // Generate password hash for "Admin@123456"
            var hasher = new PasswordHasher<ApplicationUser>();
            adminUser.PasswordHash = hasher.HashPassword(adminUser, "Admin@123456");
            return adminUser;
        }
        public static ApplicationUserRole GetAdminUserRole()
        {
            // Links the Admin User to the "Admin" Role in AspNetUserRoles
            return new ApplicationUserRole
            {
                UserId = AdminUserId,
                RoleId = AdminRoleId
            };
        }
    }
}
