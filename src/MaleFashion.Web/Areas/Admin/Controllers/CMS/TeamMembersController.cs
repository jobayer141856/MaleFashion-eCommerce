using Cortex.Mediator;
using MaleFashion.Application.Contracts.Services;
using MaleFashion.Application.Exceptions;
using MaleFashion.Application.Features.CMS.TeamMembers.Commands;
using MaleFashion.Application.Features.CMS.TeamMembers.Queries;
using MaleFashion.Domain.Entities.CMS;
using MaleFashion.Domain.Utilities;
using MaleFashion.Infrastructure.Extensions;
using MaleFashion.Web.Areas.Admin.Models;
using MaleFashion.Web.Areas.Admin.Models.CMS;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace MaleFashion.Web.Areas.Admin.Controllers.CMS
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,SuperAdmin,BlogEditor")]
    public class TeamMembersController : Controller
    {
        private readonly ILogger<TeamMembersController> _logger;
        private readonly IMediator _mediator;
        private readonly IFileStorageService _fileStorageService;

        public TeamMembersController(
            ILogger<TeamMembersController> logger,
            IMediator mediator,
            IFileStorageService fileStorageService)
        {
            _logger = logger;
            _mediator = mediator;
            _fileStorageService = fileStorageService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Create()
        {
            var model = new TeamMemberModel();
            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TeamMemberModel model, CancellationToken cancellationToken)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    string photoUrl = "malefashion/img/about/team-1.jpg";
                    if (model.Photo != null)
                    {
                        await using var stream = model.Photo.OpenReadStream();
                        photoUrl = await _fileStorageService.SaveImageAsync(
                            stream,
                            model.Photo.FileName,
                            "team",
                            cancellationToken);
                    }

                    var command = new TeamMemberAddCommand
                    {
                        Name = model.Name,
                        Role = model.Role,
                        PhotoUrl = photoUrl
                    };

                    await _mediator.SendCommandAsync(command, cancellationToken);

                    TempData.Put(Constants.ResponseTempKey,
                        new ResponseModel
                        {
                            Message = "Team member successfully created.",
                            Type = ResponseTypes.Success
                        });

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    const string errorMessage = "Failed to add team member.";
                    _logger.LogError(ex, errorMessage);
                    TempData.Put(Constants.ResponseTempKey,
                        new ResponseModel
                        {
                            Message = errorMessage,
                            Type = ResponseTypes.Danger
                        });
                }
            }
            else
            {
                TempData.Put(Constants.ResponseTempKey,
                    new ResponseModel
                    {
                        Message = "Please provide all required information.",
                        Type = ResponseTypes.Danger
                    });
            }

            return View(model);
        }

        public async Task<IActionResult> Update(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var query = new GetTeamMemberByIdQuery { Id = id };
                var member = await _mediator.SendQueryAsync(query, cancellationToken);
                if (member == null)
                {
                    TempData.Put(Constants.ResponseTempKey, new ResponseModel
                    {
                        Message = "Team member not found.",
                        Type = ResponseTypes.Danger
                    });
                    return RedirectToAction(nameof(Index));
                }

                var model = new TeamMemberModel
                {
                    Id = member.Id,
                    Name = member.Name,
                    Role = member.Role,
                    PhotoUrl = member.PhotoUrl
                };

                return View(model);
            }
            catch (Exception ex)
            {
                const string errorMessage = "Failed to load team member.";
                _logger.LogError(ex, errorMessage);
                TempData.Put(Constants.ResponseTempKey,
                    new ResponseModel
                    {
                        Message = errorMessage,
                        Type = ResponseTypes.Danger
                    });
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(TeamMemberModel model, CancellationToken cancellationToken)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var query = new GetTeamMemberByIdQuery { Id = model.Id };
                    var existing = await _mediator.SendQueryAsync(query, cancellationToken);
                    if (existing == null)
                    {
                        TempData.Put(Constants.ResponseTempKey, new ResponseModel
                        {
                            Message = "Team member does not exist.",
                            Type = ResponseTypes.Danger
                        });
                        return RedirectToAction(nameof(Index));
                    }

                    var oldPhotoUrl = existing.PhotoUrl;
                    var photoUrl = oldPhotoUrl;

                    if (model.Photo != null)
                    {
                        await using var stream = model.Photo.OpenReadStream();
                        photoUrl = await _fileStorageService.SaveImageAsync(
                            stream,
                            model.Photo.FileName,
                            "team",
                            cancellationToken);
                    }

                    var command = new TeamMemberUpdateCommand
                    {
                        Id = model.Id,
                        Name = model.Name,
                        Role = model.Role,
                        PhotoUrl = photoUrl
                    };

                    await _mediator.SendCommandAsync(command, cancellationToken);

                    // If a new photo was uploaded, clean up the old one
                    if (model.Photo != null && !string.IsNullOrEmpty(oldPhotoUrl) && oldPhotoUrl != photoUrl)
                    {
                        try
                        {
                            if (!oldPhotoUrl.StartsWith("malefashion") && !oldPhotoUrl.StartsWith("/"))
                            {
                                await _fileStorageService.DeleteImageAsync(oldPhotoUrl, "team", cancellationToken);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Failed to delete old team member photo for ID: {Id}", model.Id);
                        }
                    }

                    TempData.Put(Constants.ResponseTempKey,
                        new ResponseModel
                        {
                            Message = "Team member successfully updated.",
                            Type = ResponseTypes.Success
                        });

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    const string errorMessage = "Failed to update team member.";
                    _logger.LogError(ex, errorMessage);
                    TempData.Put(Constants.ResponseTempKey,
                        new ResponseModel
                        {
                            Message = errorMessage,
                            Type = ResponseTypes.Danger
                        });
                }
            }
            else
            {
                TempData.Put(Constants.ResponseTempKey,
                    new ResponseModel
                    {
                        Message = "Please provide all required information.",
                        Type = ResponseTypes.Danger
                    });
            }

            return View(model);
        }

        [HttpPost]
        public async Task<JsonResult> GetPagedTeamMembers([FromBody] TeamMemberListModel model, CancellationToken cancellationToken)
        {
            try
            {
                var query = new GetAllTeamMembersByPagingQuery
                {
                    PageIndex = model.PageIndex,
                    PageSize = model.PageSize,
                    SearchText = model.Search.Value ?? string.Empty,
                    SortText = model.FormatSortExpression("PhotoUrl", "Name", "Role", "CreatedAt", "Id")
                };

                var (items, total, totalDisplay) = await _mediator.SendQueryAsync<GetAllTeamMembersByPagingQuery,
                    (IList<TeamMember>, int, int)>(query, cancellationToken);

                var response = new
                {
                    recordsTotal = total,
                    recordsFiltered = totalDisplay,
                    data = (from item in items
                            select new string[]
                            {
                                !string.IsNullOrEmpty(item.PhotoUrl)
                                    ? (item.PhotoUrl.StartsWith("malefashion") || item.PhotoUrl.StartsWith("/")
                                        ? "/" + item.PhotoUrl.TrimStart('/')
                                        : $"/uploads/team/{HttpUtility.HtmlEncode(item.PhotoUrl)}")
                                    : "/malefashion/img/about/team-1.jpg",
                                HttpUtility.HtmlEncode(item.Name),
                                HttpUtility.HtmlEncode(item.Role),
                                item.CreatedAt.ToString("MMM dd, yyyy"),
                                item.Id.ToString()
                            }).ToArray()
                };

                return Json(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get team members list");
                return Json(DataTables.EmptyResult);
            }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var command = new TeamMemberDeleteCommand { Id = id };
                await _mediator.SendCommandAsync(command, cancellationToken);

                TempData.Put(Constants.ResponseTempKey,
                    new ResponseModel
                    {
                        Message = "Team member successfully deleted.",
                        Type = ResponseTypes.Success
                    });
            }
            catch (NotFoundException nfex)
            {
                TempData.Put(Constants.ResponseTempKey,
                    new ResponseModel
                    {
                        Message = nfex.Message,
                        Type = ResponseTypes.Danger
                    });
            }
            catch (Exception ex)
            {
                const string errorMessage = "Failed to delete team member.";
                _logger.LogError(ex, errorMessage);
                TempData.Put(Constants.ResponseTempKey,
                    new ResponseModel
                    {
                        Message = errorMessage,
                        Type = ResponseTypes.Danger
                    });
            }

            return RedirectToAction(nameof(Index));
        }
    }
}

