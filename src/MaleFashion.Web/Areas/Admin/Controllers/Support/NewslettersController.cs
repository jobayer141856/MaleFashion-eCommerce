using Cortex.Mediator;
using MaleFashion.Application.Exceptions;
using MaleFashion.Application.Features.Support.NewsletterSubscribers.Commands;
using MaleFashion.Application.Features.Support.NewsletterSubscribers.Queries;
using MaleFashion.Domain.Entities.Support;
using MaleFashion.Domain.Utilities;
using MaleFashion.Infrastructure.Extensions;
using MaleFashion.Web.Areas.Admin.Models;
using MaleFashion.Web.Areas.Admin.Models.Support;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace MaleFashion.Web.Areas.Admin.Controllers.Support
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,SuperAdmin,SupportStaff")]
    public class NewslettersController : Controller
    {
        private readonly IMediator _mediator;
        private readonly ILogger<NewslettersController> _logger;

        public NewslettersController(
            IMediator mediator,
            ILogger<NewslettersController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            try
            {
                var statsQuery = new GetNewsletterSubscriberStatsQuery();
                var stats = await _mediator.SendQueryAsync(statsQuery, cancellationToken);
                ViewBag.TotalSubscribers = stats.Total;
                ViewBag.ActiveSubscribers = stats.Active;
                ViewBag.InactiveSubscribers = stats.Inactive;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to load newsletter subscriber stats.");
                ViewBag.TotalSubscribers = 0;
                ViewBag.ActiveSubscribers = 0;
                ViewBag.InactiveSubscribers = 0;
            }

            return View();
        }

        [HttpPost]
        public async Task<JsonResult> GetPagedNewsletters([FromBody] NewsletterSubscriberListModel model, CancellationToken cancellationToken)
        {
            try
            {
                var query = new GetAllNewsletterSubscribersByPagingQuery
                {
                    PageIndex = model.PageIndex,
                    PageSize = model.PageSize,
                    SearchText = model.Search.Value ?? string.Empty,
                    SortText = model.FormatSortExpression("Email", "SubscribedAt", "IsActive", "Id")
                };

                var (items, total, totalDisplay) = await _mediator.SendQueryAsync<GetAllNewsletterSubscribersByPagingQuery,
                    (IList<NewsletterSubscriber>, int, int)>(query, cancellationToken);

                var response = new
                {
                    recordsTotal = total,
                    recordsFiltered = totalDisplay,
                    data = (from item in items
                            select new string[]
                            {
                                HttpUtility.HtmlEncode(item.Email),
                                item.SubscribedAt.ToString("MMM dd, yyyy HH:mm"),
                                item.IsActive ? "Active" : "Inactive",
                                item.Id.ToString()
                            }).ToArray()
                };

                return Json(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve newsletter subscribers list.");
                return Json(DataTables.EmptyResult);
            }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var command = new NewsletterSubscriberToggleStatusCommand { Id = id };
                var isActive = await _mediator.SendCommandAsync(command, cancellationToken);

                TempData.Put(Constants.ResponseTempKey,
                    new ResponseModel
                    {
                        Message = $"Subscriber successfully marked as {(isActive ? "Active" : "Inactive")}.",
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
                _logger.LogError(ex, "Failed to toggle subscriber status for ID {Id}", id);
                TempData.Put(Constants.ResponseTempKey,
                    new ResponseModel
                    {
                        Message = "Failed to update subscriber status.",
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
                var command = new NewsletterSubscriberDeleteCommand { Id = id };
                await _mediator.SendCommandAsync(command, cancellationToken);

                TempData.Put(Constants.ResponseTempKey,
                    new ResponseModel
                    {
                        Message = "Subscriber successfully deleted.",
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
                _logger.LogError(ex, "Failed to delete subscriber with ID {Id}", id);
                TempData.Put(Constants.ResponseTempKey,
                    new ResponseModel
                    {
                        Message = "Failed to delete subscriber.",
                        Type = ResponseTypes.Danger
                    });
            }

            return RedirectToAction(nameof(Index));
        }
    }
}

