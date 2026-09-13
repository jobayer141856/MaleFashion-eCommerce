using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Authorization;
using System;
using System.Linq;

namespace MaleFashion.Web.Areas.Admin.Conventions
{
    /// <summary>
    /// Model convention that automatically enforces [Authorize(Roles = "Admin,SuperAdmin")]
    /// on every controller within the "Admin" area, excluding the authentication controller.
    /// </summary>
    public class AdminAreaAuthorizationConvention : IControllerModelConvention
    {
        public void Apply(ControllerModel controller)
        {
            var isAreaAdmin = controller.Attributes
                .OfType<AreaAttribute>()
                .Any(a => string.Equals(a.RouteValue, "Admin", StringComparison.OrdinalIgnoreCase))
                || controller.RouteValues.TryGetValue("area", out var area) && string.Equals(area, "Admin", StringComparison.OrdinalIgnoreCase);

            if (!isAreaAdmin)
            {
                return;
            }

            // Exclude the Admin AccountController so its action-level attributes (e.g. [AllowAnonymous] on Login/AccessDenied) apply
            if (controller.ControllerType == typeof(Controllers.AccountController))
            {
                return;
            }

            // Require an authenticated user in Admin or SuperAdmin role
            var policy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .RequireRole("Admin", "SuperAdmin")
                .Build();

            controller.Filters.Add(new AuthorizeFilter(policy));
        }
    }
}

