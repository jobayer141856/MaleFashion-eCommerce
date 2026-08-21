using MaleFashion.Infrastructure.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace MaleFashion.Infrastructure.Data.Seeds
{
    public class RoleSeeds
    {
        public static ApplicationRole[] GetRoles()
        {
            return new ApplicationRole[]
            {
                new ApplicationRole
                {
                    Id = new Guid("00000000-0000-0000-0000-000000000001"),
                    Name = "Admin",
                    NormalizedName = "ADMIN",
                    ConcurrencyStamp = "833B5D63-C545-40F1-837F-9E175BCE0878"
                },
                new ApplicationRole
                {
                    Id = new Guid("00000000-0000-0000-0000-000000000002"),
                    Name = "Customer",
                    NormalizedName = "CUSTOMER",
                    ConcurrencyStamp= "46CC47EA-E42F-4700-B5CA-AA93D89EFDDA"
                }
            };
        }
    }
}
