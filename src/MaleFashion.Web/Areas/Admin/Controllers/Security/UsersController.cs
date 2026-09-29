using MaleFashion.Domain.Utilities;
using MaleFashion.Infrastructure.Data;
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
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UsersController> _logger;

        public UsersController(
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            ApplicationDbContext context,
            ILogger<UsersController> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var staffRoles = await _roleManager.Roles
                .Where(r => r.NormalizedName != "CUSTOMER")
                .OrderBy(r => r.Name)
                .ToListAsync();

            var model = new AdminCreateUserModel
            {
                Roles = staffRoles.Select(r => new RoleCheckboxItem
                {
                    RoleId = r.Id,
                    RoleName = r.Name ?? string.Empty,
                    IsSelected = false,
                    IsDisabled = false
                }).ToList()
            };

            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AdminCreateUserModel model)
        {
            var staffRoles = await _roleManager.Roles
                .Where(r => r.NormalizedName != "CUSTOMER")
                .OrderBy(r => r.Name)
                .ToListAsync();

            if (!ModelState.IsValid)
            {
                var selectedNames = model.Roles?.Where(r => r.IsSelected).Select(r => r.RoleName).ToHashSet(StringComparer.OrdinalIgnoreCase) ?? new();
                model.Roles = staffRoles.Select(r => new RoleCheckboxItem
                {
                    RoleId = r.Id,
                    RoleName = r.Name ?? string.Empty,
                    IsSelected = selectedNames.Contains(r.Name ?? string.Empty),
                    IsDisabled = false
                }).ToList();

                return View(model);
            }

            var existingUser = await _userManager.FindByEmailAsync(model.Email.Trim());
            if (existingUser != null)
            {
                ModelState.AddModelError(nameof(model.Email), "A user with this email address already exists.");
                var selectedNames = model.Roles?.Where(r => r.IsSelected).Select(r => r.RoleName).ToHashSet(StringComparer.OrdinalIgnoreCase) ?? new();
                model.Roles = staffRoles.Select(r => new RoleCheckboxItem
                {
                    RoleId = r.Id,
                    RoleName = r.Name ?? string.Empty,
                    IsSelected = selectedNames.Contains(r.Name ?? string.Empty),
                    IsDisabled = false
                }).ToList();
                return View(model);
            }

            var user = new ApplicationUser
            {
                UserName = model.Email.Trim(),
                Email = model.Email.Trim(),
                FirstName = model.FirstName.Trim(),
                LastName = model.LastName.Trim(),
                PhoneNumber = model.PhoneNumber?.Trim(),
                EmailConfirmed = true,
                PhoneNumberConfirmed = !string.IsNullOrWhiteSpace(model.PhoneNumber)
            };

            var createResult = await _userManager.CreateAsync(user, model.Password);
            if (!createResult.Succeeded)
            {
                foreach (var error in createResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                var selectedNames = model.Roles?.Where(r => r.IsSelected).Select(r => r.RoleName).ToHashSet(StringComparer.OrdinalIgnoreCase) ?? new();
                model.Roles = staffRoles.Select(r => new RoleCheckboxItem
                {
                    RoleId = r.Id,
                    RoleName = r.Name ?? string.Empty,
                    IsSelected = selectedNames.Contains(r.Name ?? string.Empty),
                    IsDisabled = false
                }).ToList();
                return View(model);
            }

            var selectedRoles = model.Roles?
                .Where(r => r.IsSelected)
                .Select(r => r.RoleName)
                .ToList() ?? new List<string>();

            if (selectedRoles.Any())
            {
                await _userManager.AddToRolesAsync(user, selectedRoles);
            }

            TempData.Put(Constants.ResponseTempKey, new ResponseModel
            {
                Message = "User successfully created.",
                Type = ResponseTypes.Success
            });

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<JsonResult> GetPagedUsers([FromBody] UserListModel model)
        {
            try
            {
                // Get all non-customer role IDs (administrative/operational roles)
                var staffRoleIds = await _context.Roles
                    .Where(r => r.NormalizedName != "CUSTOMER")
                    .Select(r => r.Id)
                    .ToListAsync();

                var staffUserIds = _context.UserRoles
                    .Where(ur => staffRoleIds.Contains(ur.RoleId))
                    .Select(ur => ur.UserId);

                var customerRole = await _roleManager.FindByNameAsync("Customer");
                Guid? customerRoleId = customerRole?.Id;

                var query = _userManager.Users.AsNoTracking();

                if (customerRoleId.HasValue)
                {
                    var customerUserIds = _context.UserRoles
                        .Where(ur => ur.RoleId == customerRoleId.Value)
                        .Select(ur => ur.UserId);

                    // Include users who have at least one staff role, OR newly created staff who do NOT have the Customer role
                    // Excludes pure storefront customers
                    query = query.Where(u => staffUserIds.Contains(u.Id) || !customerUserIds.Contains(u.Id));
                }
                else
                {
                    query = query.Where(u => staffUserIds.Contains(u.Id));
                }

                var total = await query.CountAsync();

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
                _logger.LogError(ex, "Failed to get staff users list");
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
                    Message = "Staff user not found.",
                    Type = ResponseTypes.Danger
                });
                return RedirectToAction(nameof(Index));
            }

            var staffRoles = await _roleManager.Roles
                .Where(r => r.NormalizedName != "CUSTOMER")
                .OrderBy(r => r.Name)
                .ToListAsync();

            var userRoles = await _userManager.GetRolesAsync(user);
            var currentUserId = _userManager.GetUserId(User);

            var model = new UserRoleAssignmentModel
            {
                UserId = user.Id,
                UserName = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                FullName = $"{user.FirstName} {user.LastName}".Trim(),
                Roles = staffRoles.Select(r => new RoleCheckboxItem
                {
                    RoleId = r.Id,
                    RoleName = r.Name ?? string.Empty,
                    IsSelected = userRoles.Contains(r.Name ?? string.Empty),
                    // Prevent self-lockout: disable unchecking Admin or SuperAdmin on yourself
                    IsDisabled = (string.Equals(r.Name, "Admin", StringComparison.OrdinalIgnoreCase) ||
                                  string.Equals(r.Name, "SuperAdmin", StringComparison.OrdinalIgnoreCase)) &&
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
                    Message = "Staff user not found.",
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
                if ((currentRoles.Contains("Admin") && !selectedRoles.Contains("Admin")) ||
                    (currentRoles.Contains("SuperAdmin") && !selectedRoles.Contains("SuperAdmin")))
                {
                    TempData.Put(Constants.ResponseTempKey, new ResponseModel
                    {
                        Message = "Security protection: You cannot remove the 'Admin' or 'SuperAdmin' role from your own active account.",
                        Type = ResponseTypes.Danger
                    });
                    return RedirectToAction(nameof(ManageRoles), new { id = model.UserId });
                }
            }

            // Only manage administrative staff roles, preserving any non-staff roles
            var staffRoles = await _roleManager.Roles
                .Where(r => r.NormalizedName != "CUSTOMER")
                .Select(r => r.Name!)
                .ToListAsync();

            var managedCurrentRoles = currentRoles
                .Where(r => staffRoles.Contains(r, StringComparer.OrdinalIgnoreCase))
                .ToList();

            var rolesToAdd = selectedRoles.Except(managedCurrentRoles, StringComparer.OrdinalIgnoreCase).ToList();
            var rolesToRemove = managedCurrentRoles.Except(selectedRoles, StringComparer.OrdinalIgnoreCase).ToList();

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
                Message = "User successfully updated.",
                Type = ResponseTypes.Success
            });

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> ResetPassword(Guid id)
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

            var roles = await _userManager.GetRolesAsync(user);
            var isTargetSuperAdmin = roles.Contains("SuperAdmin", StringComparer.OrdinalIgnoreCase);
            var currentUserIsSuperAdmin = User.IsInRole("SuperAdmin");

            // Privilege escalation safeguard: only SuperAdmin can reset a SuperAdmin's password
            if (isTargetSuperAdmin && !currentUserIsSuperAdmin)
            {
                TempData.Put(Constants.ResponseTempKey, new ResponseModel
                {
                    Message = "Access Denied: Only Super Administrators can reset passwords for accounts with the SuperAdmin role.",
                    Type = ResponseTypes.Danger
                });
                return RedirectToAction(nameof(Index));
            }

            var fullName = $"{user.FirstName} {user.LastName}".Trim();
            if (string.IsNullOrWhiteSpace(fullName)) fullName = user.UserName ?? "User";

            var model = new AdminResetUserPasswordModel
            {
                UserId = user.Id,
                Email = user.Email ?? string.Empty,
                FullName = fullName,
                RolesSummary = roles.Any() ? string.Join(", ", roles) : "No Roles",
                IsTargetSuperAdmin = isTargetSuperAdmin
            };

            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(AdminResetUserPasswordModel model)
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

            var roles = await _userManager.GetRolesAsync(user);
            var isTargetSuperAdmin = roles.Contains("SuperAdmin", StringComparer.OrdinalIgnoreCase);
            var currentUserIsSuperAdmin = User.IsInRole("SuperAdmin");

            // Privilege escalation safeguard
            if (isTargetSuperAdmin && !currentUserIsSuperAdmin)
            {
                TempData.Put(Constants.ResponseTempKey, new ResponseModel
                {
                    Message = "Access Denied: Only Super Administrators can reset passwords for accounts with the SuperAdmin role.",
                    Type = ResponseTypes.Danger
                });
                return RedirectToAction(nameof(Index));
            }

            if (!ModelState.IsValid)
            {
                var fullName = $"{user.FirstName} {user.LastName}".Trim();
                if (string.IsNullOrWhiteSpace(fullName)) fullName = user.UserName ?? "User";
                model.FullName = fullName;
                model.Email = user.Email ?? string.Empty;
                model.RolesSummary = roles.Any() ? string.Join(", ", roles) : "No Roles";
                model.IsTargetSuperAdmin = isTargetSuperAdmin;
                return View(model);
            }

            // Generate reset token and apply new password
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var resetResult = await _userManager.ResetPasswordAsync(user, token, model.NewPassword);

            if (!resetResult.Succeeded)
            {
                foreach (var error in resetResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                var fullName = $"{user.FirstName} {user.LastName}".Trim();
                if (string.IsNullOrWhiteSpace(fullName)) fullName = user.UserName ?? "User";
                model.FullName = fullName;
                model.Email = user.Email ?? string.Empty;
                model.RolesSummary = roles.Any() ? string.Join(", ", roles) : "No Roles";
                model.IsTargetSuperAdmin = isTargetSuperAdmin;
                return View(model);
            }

            // Invalidate existing sessions so target user must log in with the new password
            await _userManager.UpdateSecurityStampAsync(user);

            _logger.LogInformation("Administrator '{AdminEmail}' reset password for user '{TargetEmail}'.",
                User.Identity?.Name, user.Email);

            TempData.Put(Constants.ResponseTempKey, new ResponseModel
            {
                Message = "Password successfully reset.",
                Type = ResponseTypes.Success
            });

            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                TempData.Put(Constants.ResponseTempKey, new ResponseModel
                {
                    Message = "User account not found.",
                    Type = ResponseTypes.Danger
                });
                return RedirectToAction(nameof(Index));
            }

            var currentUserId = _userManager.GetUserId(User);

            // 1. Self-deletion safeguard
            if (string.Equals(user.Id.ToString(), currentUserId, StringComparison.OrdinalIgnoreCase))
            {
                TempData.Put(Constants.ResponseTempKey, new ResponseModel
                {
                    Message = "Security protection: You cannot delete your own active administrator account.",
                    Type = ResponseTypes.Danger
                });
                return RedirectToAction(nameof(Index));
            }

            var targetRoles = await _userManager.GetRolesAsync(user);
            var isTargetSuperAdmin = targetRoles.Contains("SuperAdmin", StringComparer.OrdinalIgnoreCase);
            var currentUserIsSuperAdmin = User.IsInRole("SuperAdmin");

            // 2. Privilege escalation safeguard: only SuperAdmin can delete a SuperAdmin
            if (isTargetSuperAdmin && !currentUserIsSuperAdmin)
            {
                TempData.Put(Constants.ResponseTempKey, new ResponseModel
                {
                    Message = "Access Denied: Only Super Administrators can delete an account with the SuperAdmin role.",
                    Type = ResponseTypes.Danger
                });
                return RedirectToAction(nameof(Index));
            }

            // 3. Sole SuperAdmin safeguard: cannot delete the last SuperAdmin
            if (isTargetSuperAdmin)
            {
                var superAdmins = await _userManager.GetUsersInRoleAsync("SuperAdmin");
                if (superAdmins.Count <= 1)
                {
                    TempData.Put(Constants.ResponseTempKey, new ResponseModel
                    {
                        Message = "Security protection: Cannot delete the last remaining SuperAdmin account in the system.",
                        Type = ResponseTypes.Danger
                    });
                    return RedirectToAction(nameof(Index));
                }
            }

            // 4. Role-assignment safeguard: prevent deletion if staff member has active roles
            if (targetRoles.Any())
            {
                TempData.Put(Constants.ResponseTempKey, new ResponseModel
                {
                    Message = $"Cannot delete staff account '{user.Email}' because it has assigned role(s): {string.Join(", ", targetRoles)}. Please unassign the role(s) first.",
                    Type = ResponseTypes.Warning
                });
                return RedirectToAction(nameof(Index));
            }

            var userEmail = user.Email;
            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                _logger.LogInformation("Administrator '{AdminEmail}' deleted staff account '{TargetEmail}' (ID: {TargetId}).",
                    User.Identity?.Name, userEmail, user.Id);

                TempData.Put(Constants.ResponseTempKey, new ResponseModel
                {
                    Message = "User successfully deleted.",
                    Type = ResponseTypes.Success
                });
            }
            else
            {
                var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                _logger.LogWarning("Failed to delete staff account '{TargetEmail}': {Errors}", userEmail, errors);

                TempData.Put(Constants.ResponseTempKey, new ResponseModel
                {
                    Message = $"Failed to delete staff account: {errors}",
                    Type = ResponseTypes.Danger
                });
            }

            return RedirectToAction(nameof(Index));
        }
    }
}

