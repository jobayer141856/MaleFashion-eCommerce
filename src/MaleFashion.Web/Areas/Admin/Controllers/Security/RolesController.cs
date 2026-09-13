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
    public class RolesController : Controller
    {
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<RolesController> _logger;

        private static readonly HashSet<string> SystemRoles = new(StringComparer.OrdinalIgnoreCase)
        {
            "Admin",
            "Customer"
        };

        public RolesController(
            RoleManager<ApplicationRole> roleManager,
            UserManager<ApplicationUser> userManager,
            ILogger<RolesController> logger)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Create()
        {
            return View(new RoleModel());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RoleModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData.Put(Constants.ResponseTempKey, new ResponseModel
                {
                    Message = "Please enter a valid role name.",
                    Type = ResponseTypes.Danger
                });
                return View(model);
            }

            var roleName = model.Name.Trim();
            if (await _roleManager.RoleExistsAsync(roleName))
            {
                TempData.Put(Constants.ResponseTempKey, new ResponseModel
                {
                    Message = $"A role with the name '{roleName}' already exists.",
                    Type = ResponseTypes.Danger
                });
                return View(model);
            }

            var role = new ApplicationRole
            {
                Name = roleName,
                ConcurrencyStamp = Guid.NewGuid().ToString()
            };

            var result = await _roleManager.CreateAsync(role);
            if (result.Succeeded)
            {
                TempData.Put(Constants.ResponseTempKey, new ResponseModel
                {
                    Message = $"Role '{roleName}' was successfully created.",
                    Type = ResponseTypes.Success
                });
                return RedirectToAction(nameof(Index));
            }

            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            TempData.Put(Constants.ResponseTempKey, new ResponseModel
            {
                Message = $"Failed to create role: {errors}",
                Type = ResponseTypes.Danger
            });

            return View(model);
        }

        public async Task<IActionResult> Update(Guid id)
        {
            var role = await _roleManager.FindByIdAsync(id.ToString());
            if (role == null)
            {
                TempData.Put(Constants.ResponseTempKey, new ResponseModel
                {
                    Message = "Role not found.",
                    Type = ResponseTypes.Danger
                });
                return RedirectToAction(nameof(Index));
            }

            if (SystemRoles.Contains(role.Name ?? string.Empty))
            {
                TempData.Put(Constants.ResponseTempKey, new ResponseModel
                {
                    Message = $"System role '{role.Name}' is protected and cannot be renamed.",
                    Type = ResponseTypes.Warning
                });
                return RedirectToAction(nameof(Index));
            }

            var model = new RoleModel
            {
                Id = role.Id,
                Name = role.Name ?? string.Empty,
                IsSystemRole = false
            };

            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(RoleModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData.Put(Constants.ResponseTempKey, new ResponseModel
                {
                    Message = "Please enter a valid role name.",
                    Type = ResponseTypes.Danger
                });
                return View(model);
            }

            var role = await _roleManager.FindByIdAsync(model.Id.ToString());
            if (role == null)
            {
                TempData.Put(Constants.ResponseTempKey, new ResponseModel
                {
                    Message = "Role not found.",
                    Type = ResponseTypes.Danger
                });
                return RedirectToAction(nameof(Index));
            }

            if (SystemRoles.Contains(role.Name ?? string.Empty))
            {
                TempData.Put(Constants.ResponseTempKey, new ResponseModel
                {
                    Message = $"System role '{role.Name}' is protected and cannot be renamed.",
                    Type = ResponseTypes.Warning
                });
                return RedirectToAction(nameof(Index));
            }

            var newName = model.Name.Trim();
            if (!string.Equals(role.Name, newName, StringComparison.OrdinalIgnoreCase) && await _roleManager.RoleExistsAsync(newName))
            {
                TempData.Put(Constants.ResponseTempKey, new ResponseModel
                {
                    Message = $"Another role with the name '{newName}' already exists.",
                    Type = ResponseTypes.Danger
                });
                return View(model);
            }

            role.Name = newName;
            var result = await _roleManager.UpdateAsync(role);
            if (result.Succeeded)
            {
                TempData.Put(Constants.ResponseTempKey, new ResponseModel
                {
                    Message = "Role name successfully updated.",
                    Type = ResponseTypes.Success
                });
                return RedirectToAction(nameof(Index));
            }

            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            TempData.Put(Constants.ResponseTempKey, new ResponseModel
            {
                Message = $"Failed to update role: {errors}",
                Type = ResponseTypes.Danger
            });

            return View(model);
        }

        [HttpPost]
        public async Task<JsonResult> GetPagedRoles([FromBody] RoleListModel model)
        {
            try
            {
                var query = _roleManager.Roles.AsNoTracking();

                if (!string.IsNullOrWhiteSpace(model.Search.Value))
                {
                    var search = model.Search.Value.Trim();
                    query = query.Where(r => r.Name != null && r.Name.Contains(search));
                }

                var total = await _roleManager.Roles.CountAsync();
                var totalDisplay = await query.CountAsync();

                var pagedRoles = await query
                    .OrderBy(r => r.Name)
                    .Skip(model.Start)
                    .Take(model.Length)
                    .ToListAsync();

                var roleData = new List<string[]>();
                foreach (var r in pagedRoles)
                {
                    var isSystem = SystemRoles.Contains(r.Name ?? string.Empty);
                    var usersInRole = await _userManager.GetUsersInRoleAsync(r.Name ?? string.Empty);
                    roleData.Add(new string[]
                    {
                        HttpUtility.HtmlEncode(r.Name ?? string.Empty),
                        usersInRole.Count.ToString(),
                        isSystem ? "System" : "Custom",
                        r.Id.ToString()
                    });
                }

                return Json(new
                {
                    recordsTotal = total,
                    recordsFiltered = totalDisplay,
                    data = roleData.ToArray()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get roles list");
                return Json(DataTables.EmptyResult);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(Guid id)
        {
            var role = await _roleManager.FindByIdAsync(id.ToString());
            if (role == null)
            {
                TempData.Put(Constants.ResponseTempKey, new ResponseModel
                {
                    Message = "Role not found.",
                    Type = ResponseTypes.Danger
                });
                return RedirectToAction(nameof(Index));
            }

            if (SystemRoles.Contains(role.Name ?? string.Empty))
            {
                TempData.Put(Constants.ResponseTempKey, new ResponseModel
                {
                    Message = $"System role '{role.Name}' is protected and cannot be deleted.",
                    Type = ResponseTypes.Danger
                });
                return RedirectToAction(nameof(Index));
            }

            var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name ?? string.Empty);
            if (usersInRole.Count > 0)
            {
                TempData.Put(Constants.ResponseTempKey, new ResponseModel
                {
                    Message = $"Cannot delete role '{role.Name}' because {usersInRole.Count} user(s) are currently assigned to it. Please reassign the users first.",
                    Type = ResponseTypes.Warning
                });
                return RedirectToAction(nameof(Index));
            }

            var result = await _roleManager.DeleteAsync(role);
            if (result.Succeeded)
            {
                TempData.Put(Constants.ResponseTempKey, new ResponseModel
                {
                    Message = $"Role '{role.Name}' was successfully deleted.",
                    Type = ResponseTypes.Success
                });
            }
            else
            {
                var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                TempData.Put(Constants.ResponseTempKey, new ResponseModel
                {
                    Message = $"Failed to delete role: {errors}",
                    Type = ResponseTypes.Danger
                });
            }

            return RedirectToAction(nameof(Index));
        }
    }
}

