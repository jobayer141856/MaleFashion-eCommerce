using MaleFashion.Domain.Utilities;
using MaleFashion.Infrastructure.Extensions;
using MaleFashion.Infrastructure.Identity;
using MaleFashion.Web.Areas.Admin.Models;
using MaleFashion.Web.Areas.Admin.Models.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace MaleFashion.Web.Areas.Admin.Controllers.Security
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly ILogger<UsersController> _logger;

        public UsersController(
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            ILogger<UsersController> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<JsonResult> GetPagedUsers([FromBody] UserListModel model)
        {
            try
            {
                var query = _userManager.Users.AsNoTracking();

                if (!string.IsNullOrWhiteSpace(model.Search.Value))
                {
                    var search = model.Search.Value.Trim().ToLower();
                    query = query.Where(u =>
                        (u.Email != null && u.Email.ToLower().Contains(search)) ||
                        (u.UserName != null && u.UserName.ToLower().Contains(search)) ||
                        (u.FirstName != null && u.FirstName.ToLower().Contains(search)) ||
                        (u.LastName != null && u.LastName.ToLower().Contains(search)) ||
                        (u.PhoneNumber != null && u.PhoneNumber.Contains(search)));
                }

                var total = await _userManager.Users.CountAsync();
                var totalDisplay = await query.CountAsync();

                var users = await query
                    .OrderBy(u => u.Email)
                    .Skip(model.Start)
                    .Take(model.Length)
                    .ToListAsync();

                var userData = new List<string[]>();
                foreach (var user in users)
                {
                    var fullName = $"{user.FirstName} {user.LastName}".Trim();
                    if (string.IsNullOrWhiteSpace(fullName)) fullName = user.UserName ?? "—";

                    var roles = await _userManager.GetRolesAsync(user);
                    var rolesDisplay = roles.Any()
                        ? string.Join(", ", roles)
                        : "No Roles";

                    userData.Add(new string[]
                    {
                        HttpUtility.HtmlEncode(fullName),
                        HttpUtility.HtmlEncode(user.Email ?? "—"),
                        HttpUtility.HtmlEncode(user.PhoneNumber ?? "—"),
                        HttpUtility.HtmlEncode(rolesDisplay),
                        user.EmailConfirmed ? "Confirmed" : "Pending",
                        user.Id.ToString()
                    });
                }

                return Json(new
                {
                    recordsTotal = total,
                    recordsFiltered = totalDisplay,
                    data = userData.ToArray()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get users list");
                return Json(DataTables.EmptyResult);
            }
        }

        public async Task<IActionResult> ManageRoles(Guid id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                TempData.Put(Constants.ResponseTempKey, new ResponseModel
                {
                    Message = "User not found.",
                    Type = ResponseTypes.Danger
                });
                return RedirectToAction(nameof(Index));
            }

            var allRoles = await _roleManager.Roles.OrderBy(r => r.Name).ToListAsync();
            var userRoles = await _userManager.GetRolesAsync(user);
            var currentUserId = _userManager.GetUserId(User);

            var model = new UserRoleAssignmentModel
            {
                UserId = user.Id,
                UserName = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                FullName = $"{user.FirstName} {user.LastName}".Trim(),
                Roles = allRoles.Select(r => new RoleCheckboxItem
                {
                    RoleId = r.Id,
                    RoleName = r.Name ?? string.Empty,
                    IsSelected = userRoles.Contains(r.Name ?? string.Empty),
                    // Prevent self-lockout: disable unchecking Admin on yourself
                    IsDisabled = string.Equals(r.Name, "Admin", StringComparison.OrdinalIgnoreCase) &&
                                 string.Equals(user.Id.ToString(), currentUserId, StringComparison.OrdinalIgnoreCase)
                }).ToList()
            };

            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ManageRoles(UserRoleAssignmentModel model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId.ToString());
            if (user == null)
            {
                TempData.Put(Constants.ResponseTempKey, new ResponseModel
                {
                    Message = "User not found.",
                    Type = ResponseTypes.Danger
                });
                return RedirectToAction(nameof(Index));
            }

            var currentUserId = _userManager.GetUserId(User);
            var currentRoles = await _userManager.GetRolesAsync(user);
            var selectedRoles = model.Roles
                .Where(r => r.IsSelected)
                .Select(r => r.RoleName)
                .ToList();

            // Self-lockout safeguard
            if (string.Equals(user.Id.ToString(), currentUserId, StringComparison.OrdinalIgnoreCase))
            {
                if (currentRoles.Contains("Admin") && !selectedRoles.Contains("Admin"))
                {
                    TempData.Put(Constants.ResponseTempKey, new ResponseModel
                    {
                        Message = "Security protection: You cannot remove the 'Admin' role from your own active account.",
                        Type = ResponseTypes.Danger
                    });
                    return RedirectToAction(nameof(ManageRoles), new { id = model.UserId });
                }
            }

            var rolesToAdd = selectedRoles.Except(currentRoles, StringComparer.OrdinalIgnoreCase).ToList();
            var rolesToRemove = currentRoles.Except(selectedRoles, StringComparer.OrdinalIgnoreCase).ToList();

            if (rolesToAdd.Any())
            {
                var addResult = await _userManager.AddToRolesAsync(user, rolesToAdd);
                if (!addResult.Succeeded)
                {
                    var errors = string.Join("; ", addResult.Errors.Select(e => e.Description));
                    TempData.Put(Constants.ResponseTempKey, new ResponseModel
                    {
                        Message = $"Failed to add roles: {errors}",
                        Type = ResponseTypes.Danger
                    });
                    return RedirectToAction(nameof(ManageRoles), new { id = model.UserId });
                }
            }

            if (rolesToRemove.Any())
            {
                var removeResult = await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
                if (!removeResult.Succeeded)
                {
                    var errors = string.Join("; ", removeResult.Errors.Select(e => e.Description));
                    TempData.Put(Constants.ResponseTempKey, new ResponseModel
                    {
                        Message = $"Failed to remove roles: {errors}",
                        Type = ResponseTypes.Danger
                    });
                    return RedirectToAction(nameof(ManageRoles), new { id = model.UserId });
                }
            }

            TempData.Put(Constants.ResponseTempKey, new ResponseModel
            {
                Message = $"Roles successfully updated for user '{user.Email}'.",
                Type = ResponseTypes.Success
            });

            return RedirectToAction(nameof(Index));
        }
    }
}

