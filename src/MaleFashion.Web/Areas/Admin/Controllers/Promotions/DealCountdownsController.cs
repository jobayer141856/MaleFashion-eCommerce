using Cortex.Mediator;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Exceptions;
using MaleFashion.Application.Features.Promotions.DealCountdowns.Commands;
using MaleFashion.Application.Features.Promotions.DealCountdowns.Queries;
using MaleFashion.Domain.Entities.Promotions;
using MaleFashion.Domain.Utilities;
using MaleFashion.Infrastructure.Extensions;
using MaleFashion.Web.Areas.Admin.Models;
using MaleFashion.Web.Areas.Admin.Models.Promotions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace MaleFashion.Web.Areas.Admin.Controllers.Promotions
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,SuperAdmin,CatalogManager")]
    public class DealCountdownsController : Controller
    {
        private readonly IMediator _mediator;
        private readonly IApplicationUnitOfWork _unitOfWork;
        private readonly ILogger<DealCountdownsController> _logger;

        public DealCountdownsController(
            IMediator mediator,
            IApplicationUnitOfWork unitOfWork,
            ILogger<DealCountdownsController> logger)
        {
            _mediator = mediator;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            try
            {
                var stats = await _mediator.SendQueryAsync(new GetDealCountdownStatsQuery(), cancellationToken);
                ViewBag.TotalDeals = stats.Total;
                ViewBag.LiveDeals = stats.Live;
                ViewBag.UpcomingDeals = stats.Upcoming;
                ViewBag.ExpiredDeals = stats.Expired;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to load deal stats.");
                ViewBag.TotalDeals = 0;
                ViewBag.LiveDeals = 0;
                ViewBag.UpcomingDeals = 0;
                ViewBag.ExpiredDeals = 0;
            }

            return View();
        }

        public async Task<IActionResult> Create(CancellationToken cancellationToken)
        {
            ViewBag.Products = await _mediator.SendQueryAsync(new GetEligibleProductsForDealQuery(), cancellationToken);
            var model = new DealCountdownModel
            {
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(7)
            };
            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DealCountdownModel model, CancellationToken cancellationToken)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var command = new DealCountdownAddCommand
                    {
                        ProductId = model.ProductId,
                        DealTitle = model.DealTitle,
                        SalePrice = model.SalePrice,
                        Currency = model.Currency ?? "BDT",
                        StartDate = model.StartDate,
                        EndDate = model.EndDate,
                        IsActive = model.IsActive
                    };

                    await _mediator.SendCommandAsync(command, cancellationToken);

                    TempData.Put(Constants.ResponseTempKey,
                        new ResponseModel
                        {
                            Message = "Flash deal successfully scheduled.",
                            Type = ResponseTypes.Success
                        });

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to create flash deal.");
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

            ViewBag.Products = await _mediator.SendQueryAsync(new GetEligibleProductsForDealQuery(), cancellationToken);
            return View(model);
        }

        public async Task<IActionResult> Update(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var deal = await _mediator.SendQueryAsync(new GetDealCountdownByIdQuery { Id = id }, cancellationToken);
                if (deal == null)
                {
                    TempData.Put(Constants.ResponseTempKey,
                        new ResponseModel
                        {
                            Message = "Flash deal not found.",
                            Type = ResponseTypes.Danger
                        });
                    return RedirectToAction(nameof(Index));
                }

                ViewBag.Products = await _mediator.SendQueryAsync(new GetEligibleProductsForDealQuery(), cancellationToken);

                var model = new DealCountdownModel
                {
                    Id = deal.Id,
                    ProductId = deal.ProductId,
                    ProductName = deal.Product?.Name,
                    ProductSku = deal.Product?.Sku,
                    ProductBasePrice = deal.Product?.BasePrice?.Amount,
                    DealTitle = deal.DealTitle,
                    SalePrice = deal.SalePrice?.Amount ?? 0,
                    Currency = deal.SalePrice?.Currency ?? "BDT",
                    StartDate = deal.StartDate,
                    EndDate = deal.EndDate,
                    IsActive = deal.IsActive
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load deal countdown with ID {Id}", id);
                TempData.Put(Constants.ResponseTempKey,
                    new ResponseModel
                    {
                        Message = "Failed to load flash deal.",
                        Type = ResponseTypes.Danger
                    });
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(DealCountdownModel model, CancellationToken cancellationToken)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var command = new DealCountdownUpdateCommand
                    {
                        Id = model.Id,
                        ProductId = model.ProductId,
                        DealTitle = model.DealTitle,
                        SalePrice = model.SalePrice,
                        Currency = model.Currency ?? "BDT",
                        StartDate = model.StartDate,
                        EndDate = model.EndDate,
                        IsActive = model.IsActive
                    };

                    await _mediator.SendCommandAsync(command, cancellationToken);

                    TempData.Put(Constants.ResponseTempKey,
                        new ResponseModel
                        {
                            Message = "Flash deal successfully updated.",
                            Type = ResponseTypes.Success
                        });

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to update flash deal.");
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

            ViewBag.Products = await _mediator.SendQueryAsync(new GetEligibleProductsForDealQuery(), cancellationToken);
            return View(model);
        }

        [HttpPost]
        public async Task<JsonResult> GetPagedDealCountdowns([FromBody] DealCountdownListModel model, CancellationToken cancellationToken)
        {
            try
            {
                var query = new GetAllDealCountdownsByPagingQuery
                {
                    PageIndex = model.PageIndex,
                    PageSize = model.PageSize,
                    SearchText = model.Search.Value ?? string.Empty,
                    SortText = model.FormatSortExpression("Product.Name", "DealTitle", "SalePrice.Amount", "EndDate", "IsActive", "Id")
                };

                var (items, total, totalDisplay) = await _mediator.SendQueryAsync<GetAllDealCountdownsByPagingQuery,
                    (IList<DealCountdown>, int, int)>(query, cancellationToken);

                var now = DateTime.UtcNow;

                var response = new
                {
                    recordsTotal = total,
                    recordsFiltered = totalDisplay,
                    data = (from item in items
                            let basePrice = item.Product?.BasePrice?.Amount ?? 0
                            let salePrice = item.SalePrice?.Amount ?? 0
                            let currency = item.SalePrice?.Currency ?? "BDT"
                            let discountPct = (basePrice > 0 && salePrice < basePrice)
                                ? Math.Round((1 - (salePrice / basePrice)) * 100)
                                : 0
                            let imgUrl = item.Product?.Images?.FirstOrDefault()?.ImageUrl
                            let imgTag = !string.IsNullOrEmpty(imgUrl)
                                ? $"/uploads/products/{HttpUtility.HtmlEncode(imgUrl)}"
                                : "/img/product/product-1.jpg"
                            let statusText = !item.IsActive
                                ? "Inactive"
                                : (item.EndDate < now ? "Expired" : (item.StartDate.HasValue && item.StartDate.Value > now ? "Upcoming" : "Live"))
                            let productHtml = $@"<div class='d-flex align-items-center'>
                                <img src='{imgTag}' class='rounded me-2' style='width:45px;height:45px;object-fit:cover;' />
                                <div>
                                    <span class='fw-semibold d-block'>{HttpUtility.HtmlEncode(item.Product?.Name ?? "N/A")}</span>
                                    <small class='text-muted'>SKU: {HttpUtility.HtmlEncode(item.Product?.Sku ?? "N/A")}</small>
                                </div>
                            </div>"
                            let priceHtml = $@"<div>
                                <span class='fw-bold text-danger'>{salePrice:N2} {currency}</span>
                                <span class='text-decoration-line-through text-muted small ms-1'>{basePrice:N2}</span>
                                <span class='badge bg-label-danger ms-1'>-{discountPct:0}%</span>
                            </div>"
                            select new string[]
                            {
                                productHtml,
                                HttpUtility.HtmlEncode(item.DealTitle),
                                priceHtml,
                                item.EndDate.ToString("MMM dd, yyyy HH:mm"),
                                statusText,
                                item.Id.ToString()
                            }).ToArray()
                };

                return Json(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get deal countdowns list.");
                return Json(DataTables.EmptyResult);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetProductPrice(Guid productId, CancellationToken cancellationToken)
        {
            var product = await _unitOfWork.ProductRepository.GetByIdAsync(productId, cancellationToken);
            if (product == null)
                return NotFound(new { message = "Product not found" });

            return Json(new
            {
                basePrice = product.BasePrice?.Amount ?? 0,
                currency = product.BasePrice?.Currency ?? "BDT",
                name = product.Name,
                sku = product.Sku
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var command = new DealCountdownToggleStatusCommand { Id = id };
                var isActive = await _mediator.SendCommandAsync(command, cancellationToken);

                TempData.Put(Constants.ResponseTempKey,
                    new ResponseModel
                    {
                        Message = $"Flash deal marked as {(isActive ? "Active" : "Inactive")}.",
                        Type = ResponseTypes.Success
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to toggle deal status for ID {Id}", id);
                TempData.Put(Constants.ResponseTempKey,
                    new ResponseModel
                    {
                        Message = "Failed to update deal status.",
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
                var command = new DealCountdownDeleteCommand { Id = id };
                await _mediator.SendCommandAsync(command, cancellationToken);

                TempData.Put(Constants.ResponseTempKey,
                    new ResponseModel
                    {
                        Message = "Flash deal successfully deleted.",
                        Type = ResponseTypes.Success
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete flash deal ID {Id}", id);
                TempData.Put(Constants.ResponseTempKey,
                    new ResponseModel
                    {
                        Message = ex.Message,
                        Type = ResponseTypes.Danger
                    });
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
