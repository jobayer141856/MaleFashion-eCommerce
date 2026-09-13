using Cortex.Mediator;
using MaleFashion.Application.Contracts.Services;
using MaleFashion.Application.Exceptions;
using MaleFashion.Application.Features.CMS.Testimonials.Commands;
using MaleFashion.Application.Features.CMS.Testimonials.Queries;
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
    public class TestimonialsController : Controller
    {
        private readonly ILogger<TestimonialsController> _logger;
        private readonly IMediator _mediator;
        private readonly IFileStorageService _fileStorageService;

        public TestimonialsController(
            ILogger<TestimonialsController> logger,
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
            var model = new TestimonialModel();
            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TestimonialModel model, CancellationToken cancellationToken)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    string? avatarUrl = null;
                    if (model.Avatar != null)
                    {
                        await using var stream = model.Avatar.OpenReadStream();
                        avatarUrl = await _fileStorageService.SaveImageAsync(
                            stream,
                            model.Avatar.FileName,
                            "testimonials",
                            cancellationToken);
                    }

                    var command = new TestimonialAddCommand
                    {
                        AuthorName = model.AuthorName,
                        AuthorProfession = model.AuthorProfession,
                        Quote = model.Quote,
                        AvatarUrl = avatarUrl
                    };

                    await _mediator.SendCommandAsync(command, cancellationToken);

                    TempData.Put(Constants.ResponseTempKey,
                        new ResponseModel
                        {
                            Message = "Testimonial successfully added.",
                            Type = ResponseTypes.Success
                        });

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    const string errorMessage = "Failed to add testimonial.";
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
                var query = new GetTestimonialByIdQuery { Id = id };
                var testimonial = await _mediator.SendQueryAsync(query, cancellationToken);
                if (testimonial == null)
                {
                    TempData.Put(Constants.ResponseTempKey, new ResponseModel
                    {
                        Message = "Testimonial not found.",
                        Type = ResponseTypes.Danger
                    });
                    return RedirectToAction(nameof(Index));
                }

                var model = new TestimonialModel
                {
                    Id = testimonial.Id,
                    AuthorName = testimonial.AuthorName,
                    AuthorProfession = testimonial.AuthorProfession,
                    Quote = testimonial.Quote,
                    AvatarUrl = testimonial.AvatarUrl
                };

                return View(model);
            }
            catch (Exception ex)
            {
                const string errorMessage = "Failed to load testimonial.";
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
        public async Task<IActionResult> Update(TestimonialModel model, CancellationToken cancellationToken)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var query = new GetTestimonialByIdQuery { Id = model.Id };
                    var existing = await _mediator.SendQueryAsync(query, cancellationToken);
                    if (existing == null)
                    {
                        TempData.Put(Constants.ResponseTempKey, new ResponseModel
                        {
                            Message = "Testimonial does not exist.",
                            Type = ResponseTypes.Danger
                        });
                        return RedirectToAction(nameof(Index));
                    }

                    var oldAvatarUrl = existing.AvatarUrl;
                    var avatarUrl = oldAvatarUrl;

                    if (model.Avatar != null)
                    {
                        await using var stream = model.Avatar.OpenReadStream();
                        avatarUrl = await _fileStorageService.SaveImageAsync(
                            stream,
                            model.Avatar.FileName,
                            "testimonials",
                            cancellationToken);
                    }

                    var command = new TestimonialUpdateCommand
                    {
                        Id = model.Id,
                        AuthorName = model.AuthorName,
                        AuthorProfession = model.AuthorProfession,
                        Quote = model.Quote,
                        AvatarUrl = avatarUrl
                    };

                    await _mediator.SendCommandAsync(command, cancellationToken);

                    // Clean up old avatar if updated
                    if (model.Avatar != null && !string.IsNullOrEmpty(oldAvatarUrl) && oldAvatarUrl != avatarUrl)
                    {
                        try
                        {
                            await _fileStorageService.DeleteImageAsync(oldAvatarUrl, "testimonials", cancellationToken);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Failed to delete old testimonial avatar for ID: {Id}", model.Id);
                        }
                    }

                    TempData.Put(Constants.ResponseTempKey,
                        new ResponseModel
                        {
                            Message = "Testimonial successfully updated.",
                            Type = ResponseTypes.Success
                        });

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    const string errorMessage = "Failed to update testimonial.";
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
        public async Task<JsonResult> GetPagedTestimonials([FromBody] TestimonialListModel model, CancellationToken cancellationToken)
        {
            try
            {
                var query = new GetAllTestimonialsByPagingQuery
                {
                    PageIndex = model.PageIndex,
                    PageSize = model.PageSize,
                    SearchText = model.Search.Value ?? string.Empty,
                    SortText = model.FormatSortExpression("AvatarUrl", "AuthorName", "AuthorProfession", "Quote", "CreatedAt", "Id")
                };

                var (items, total, totalDisplay) = await _mediator.SendQueryAsync<GetAllTestimonialsByPagingQuery,
                    (IList<Testimonial>, int, int)>(query, cancellationToken);

                var response = new
                {
                    recordsTotal = total,
                    recordsFiltered = totalDisplay,
                    data = (from item in items
                            select new string[]
                            {
                                !string.IsNullOrEmpty(item.AvatarUrl)
                                    ? $"/uploads/testimonials/{HttpUtility.HtmlEncode(item.AvatarUrl)}"
                                    : "/img/about/testimonial-author.jpg",
                                HttpUtility.HtmlEncode(item.AuthorName),
                                HttpUtility.HtmlEncode(item.AuthorProfession),
                                HttpUtility.HtmlEncode(item.Quote.Length > 60 ? item.Quote.Substring(0, 57) + "..." : item.Quote),
                                item.CreatedAt.ToString("MMM dd, yyyy"),
                                item.Id.ToString()
                            }).ToArray()
                };

                return Json(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get testimonial list");
                return Json(DataTables.EmptyResult);
            }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var command = new TestimonialDeleteCommand { Id = id };
                await _mediator.SendCommandAsync(command, cancellationToken);

                TempData.Put(Constants.ResponseTempKey,
                    new ResponseModel
                    {
                        Message = "Testimonial successfully deleted.",
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
                const string errorMessage = "Failed to delete testimonial.";
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

