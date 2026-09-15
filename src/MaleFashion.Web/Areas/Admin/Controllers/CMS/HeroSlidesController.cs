using Cortex.Mediator;
using MaleFashion.Application.Contracts.Services;
using MaleFashion.Application.Exceptions;
using MaleFashion.Application.Features.CMS.HeroSlides.Commands;
using MaleFashion.Application.Features.CMS.HeroSlides.Queries;
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
    public class HeroSlidesController : Controller
    {
        private readonly ILogger<HeroSlidesController> _logger;
        private readonly IMediator _mediator;
        private readonly IFileStorageService _fileStorageService;

        public HeroSlidesController(
            ILogger<HeroSlidesController> logger,
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
            var model = new HeroSlideModel();
            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(HeroSlideModel model, CancellationToken cancellationToken)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    string imageUrl = "malefashion/img/hero/hero-1.jpg";
                    if (model.Photo != null)
                    {
                        await using var stream = model.Photo.OpenReadStream();
                        imageUrl = await _fileStorageService.SaveImageAsync(
                            stream,
                            model.Photo.FileName,
                            "hero",
                            cancellationToken);
                    }

                    var command = new HeroSlideAddCommand
                    {
                        Subtitle = model.Subtitle,
                        Title = model.Title,
                        Description = model.Description,
                        ImageUrl = imageUrl,
                        ButtonText = model.ButtonText,
                        TargetUrl = model.TargetUrl,
                        DisplayOrder = model.DisplayOrder,
                        IsActive = model.IsActive
                    };

                    await _mediator.SendCommandAsync(command, cancellationToken);

                    TempData.Put(Constants.ResponseTempKey,
                        new ResponseModel
                        {
                            Message = "Hero slide successfully added.",
                            Type = ResponseTypes.Success
                        });

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to create hero slide.");
                    TempData.Put(Constants.ResponseTempKey,
                        new ResponseModel
                        {
                            Message = ex.Message,
                            Type = ResponseTypes.Danger
                        });
                }
            }
            else
            {
                TempData.Put(Constants.ResponseTempKey,
                    new ResponseModel
                    {
                        Message = "Please correct the errors in the form.",
                        Type = ResponseTypes.Danger
                    });
            }

            return View(model);
        }

        public async Task<IActionResult> Update(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var slide = await _mediator.SendQueryAsync(new GetHeroSlideByIdQuery { Id = id }, cancellationToken);
                if (slide == null)
                {
                    TempData.Put(Constants.ResponseTempKey,
                        new ResponseModel
                        {
                            Message = "Hero slide not found.",
                            Type = ResponseTypes.Danger
                        });
                    return RedirectToAction(nameof(Index));
                }

                var model = new HeroSlideModel
                {
                    Id = slide.Id,
                    Subtitle = slide.Subtitle,
                    Title = slide.Title,
                    Description = slide.Description,
                    ButtonText = slide.ButtonText,
                    TargetUrl = slide.TargetUrl,
                    DisplayOrder = slide.DisplayOrder,
                    IsActive = slide.IsActive,
                    ImageUrl = slide.ImageUrl
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load hero slide with ID {Id}", id);
                TempData.Put(Constants.ResponseTempKey,
                    new ResponseModel
                    {
                        Message = "Failed to load hero slide.",
                        Type = ResponseTypes.Danger
                    });
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(HeroSlideModel model, CancellationToken cancellationToken)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    string? newImageName = null;
                    if (model.Photo != null)
                    {
                        await using var stream = model.Photo.OpenReadStream();
                        newImageName = await _fileStorageService.SaveImageAsync(
                            stream,
                            model.Photo.FileName,
                            "hero",
                            cancellationToken);
                    }

                    var command = new HeroSlideUpdateCommand
                    {
                        Id = model.Id,
                        Subtitle = model.Subtitle,
                        Title = model.Title,
                        Description = model.Description,
                        ImageUrl = newImageName,
                        ButtonText = model.ButtonText,
                        TargetUrl = model.TargetUrl,
                        DisplayOrder = model.DisplayOrder,
                        IsActive = model.IsActive
                    };

                    await _mediator.SendCommandAsync(command, cancellationToken);

                    TempData.Put(Constants.ResponseTempKey,
                        new ResponseModel
                        {
                            Message = "Hero slide successfully updated.",
                            Type = ResponseTypes.Success
                        });

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to update hero slide.");
                    TempData.Put(Constants.ResponseTempKey,
                        new ResponseModel
                        {
                            Message = ex.Message,
                            Type = ResponseTypes.Danger
                        });
                }
            }
            else
            {
                TempData.Put(Constants.ResponseTempKey,
                    new ResponseModel
                    {
                        Message = "Please correct the errors in the form.",
                        Type = ResponseTypes.Danger
                    });
            }

            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var command = new HeroSlideToggleStatusCommand { Id = id };
                var isActive = await _mediator.SendCommandAsync(command, cancellationToken);

                TempData.Put(Constants.ResponseTempKey,
                    new ResponseModel
                    {
                        Message = $"Hero slide marked as {(isActive ? "Active" : "Inactive")}.",
                        Type = ResponseTypes.Success
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to toggle hero slide status for ID {Id}", id);
                TempData.Put(Constants.ResponseTempKey,
                    new ResponseModel
                    {
                        Message = "Failed to update slide status.",
                        Type = ResponseTypes.Danger
                    });
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var command = new HeroSlideDeleteCommand { Id = id };
                await _mediator.SendCommandAsync(command, cancellationToken);

                TempData.Put(Constants.ResponseTempKey,
                    new ResponseModel
                    {
                        Message = "Hero slide successfully deleted.",
                        Type = ResponseTypes.Success
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete hero slide ID {Id}", id);
                TempData.Put(Constants.ResponseTempKey,
                    new ResponseModel
                    {
                        Message = ex.Message,
                        Type = ResponseTypes.Danger
                    });
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<JsonResult> GetPagedHeroSlides([FromBody] HeroSlideListModel model, CancellationToken cancellationToken)
        {
            try
            {
                var query = new GetAllHeroSlidesByPagingQuery
                {
                    PageIndex = model.PageIndex,
                    PageSize = model.PageSize,
                    SearchText = model.Search.Value ?? string.Empty,
                    SortText = model.FormatSortExpression("Title", "Subtitle", "DisplayOrder", "IsActive", "CreatedAt")
                };

                var (items, total, totalDisplay) = await _mediator.SendQueryAsync<GetAllHeroSlidesByPagingQuery,
                    (IList<HeroSlide>, int, int)>(query, cancellationToken);

                var response = new
                {
                    recordsTotal = total,
                    recordsFiltered = totalDisplay,
                    data = (from item in items
                            let imgUrl = ResolveImageUrl(item.ImageUrl)
                            let statusBadge = item.IsActive
                                ? "<span class='badge bg-label-success'>Active</span>"
                                : "<span class='badge bg-label-secondary'>Inactive</span>"
                            select new string[]
                            {
                                imgUrl,
                                HttpUtility.HtmlEncode(item.Subtitle),
                                HttpUtility.HtmlEncode(item.Title),
                                item.DisplayOrder.ToString(),
                                statusBadge,
                                item.Id.ToString()
                            }).ToArray()
                };

                return Json(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get hero slides list.");
                return Json(DataTables.EmptyResult);
            }
        }

        private static string ResolveImageUrl(string? path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return "/malefashion/img/hero/hero-1.jpg";

            if (path.StartsWith("http", StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith("/", StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith("~", StringComparison.OrdinalIgnoreCase))
            {
                return path.TrimStart('~');
            }

            return $"/uploads/hero/{path}";
        }
    }
}

