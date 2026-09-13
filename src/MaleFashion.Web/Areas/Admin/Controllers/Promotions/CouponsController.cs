using Cortex.Mediator;
using MaleFashion.Application.Exceptions;
using MaleFashion.Application.Features.Promotions.Coupons.Commands;
using MaleFashion.Application.Features.Promotions.Coupons.Queries;
using MaleFashion.Domain.Entities.Promotions;
using MaleFashion.Domain.Enums;
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
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class CouponsController : Controller
    {
        private readonly IMediator _mediator;
        private readonly ILogger<CouponsController> _logger;

        public CouponsController(
            IMediator mediator,
            ILogger<CouponsController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            try
            {
                var statsQuery = new GetCouponStatsQuery();
                var stats = await _mediator.SendQueryAsync(statsQuery, cancellationToken);
                ViewBag.TotalCoupons = stats.Total;
                ViewBag.ActiveCoupons = stats.Active;
                ViewBag.ExpiredCoupons = stats.Expired;
                ViewBag.TotalRedemptions = stats.TotalUsed;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to load coupon stats.");
                ViewBag.TotalCoupons = 0;
                ViewBag.ActiveCoupons = 0;
                ViewBag.ExpiredCoupons = 0;
                ViewBag.TotalRedemptions = 0;
            }

            return View();
        }

        public IActionResult Create()
        {
            var model = new CouponModel
            {
                StartDate = DateTime.UtcNow,
                ExpirationDate = DateTime.UtcNow.AddDays(30)
            };
            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CouponModel model, CancellationToken cancellationToken)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var command = new CouponAddCommand
                    {
                        Code = model.Code,
                        DiscountType = model.DiscountType,
                        DiscountValue = model.DiscountValue,
                        MinimumSpend = model.MinimumSpend,
                        UsageLimit = model.UsageLimit,
                        StartDate = model.StartDate,
                        ExpirationDate = model.ExpirationDate,
                        IsActive = model.IsActive
                    };

                    await _mediator.SendCommandAsync(command, cancellationToken);

                    TempData.Put(Constants.ResponseTempKey,
                        new ResponseModel
                        {
                            Message = "Coupon successfully created.",
                            Type = ResponseTypes.Success
                        });

                    return RedirectToAction(nameof(Index));
                }
                catch (DuplicateDataException dex)
                {
                    TempData.Put(Constants.ResponseTempKey,
                        new ResponseModel
                        {
                            Message = dex.Message,
                            Type = ResponseTypes.Danger
                        });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to create coupon.");
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
                var query = new GetCouponByIdQuery { Id = id };
                var coupon = await _mediator.SendQueryAsync(query, cancellationToken);
                if (coupon == null)
                {
                    TempData.Put(Constants.ResponseTempKey,
                        new ResponseModel
                        {
                            Message = "Coupon not found.",
                            Type = ResponseTypes.Danger
                        });
                    return RedirectToAction(nameof(Index));
                }

                var model = new CouponModel
                {
                    Id = coupon.Id,
                    Code = coupon.Code,
                    DiscountType = coupon.DiscountType,
                    DiscountValue = coupon.DiscountValue,
                    MinimumSpend = coupon.MinimumSpend,
                    UsageLimit = coupon.UsageLimit,
                    UsedCount = coupon.UsedCount,
                    StartDate = coupon.StartDate,
                    ExpirationDate = coupon.ExpirationDate,
                    IsActive = coupon.IsActive
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load coupon with ID {Id}", id);
                TempData.Put(Constants.ResponseTempKey,
                    new ResponseModel
                    {
                        Message = "Failed to load coupon.",
                        Type = ResponseTypes.Danger
                    });
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(CouponModel model, CancellationToken cancellationToken)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var command = new CouponUpdateCommand
                    {
                        Id = model.Id,
                        Code = model.Code,
                        DiscountType = model.DiscountType,
                        DiscountValue = model.DiscountValue,
                        MinimumSpend = model.MinimumSpend,
                        UsageLimit = model.UsageLimit,
                        StartDate = model.StartDate,
                        ExpirationDate = model.ExpirationDate,
                        IsActive = model.IsActive
                    };

                    await _mediator.SendCommandAsync(command, cancellationToken);

                    TempData.Put(Constants.ResponseTempKey,
                        new ResponseModel
                        {
                            Message = "Coupon successfully updated.",
                            Type = ResponseTypes.Success
                        });

                    return RedirectToAction(nameof(Index));
                }
                catch (DuplicateDataException dex)
                {
                    TempData.Put(Constants.ResponseTempKey,
                        new ResponseModel
                        {
                            Message = dex.Message,
                            Type = ResponseTypes.Danger
                        });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to update coupon.");
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

        [HttpPost]
        public async Task<JsonResult> GetPagedCoupons([FromBody] CouponListModel model, CancellationToken cancellationToken)
        {
            try
            {
                var query = new GetAllCouponsByPagingQuery
                {
                    PageIndex = model.PageIndex,
                    PageSize = model.PageSize,
                    SearchText = model.Search.Value ?? string.Empty,
                    SortText = model.FormatSortExpression("Code", "DiscountType", "DiscountValue", "ExpirationDate", "UsedCount", "IsActive", "Id")
                };

                var (items, total, totalDisplay) = await _mediator.SendQueryAsync<GetAllCouponsByPagingQuery,
                    (IList<Coupon>, int, int)>(query, cancellationToken);

                var now = DateTime.UtcNow;

                var response = new
                {
                    recordsTotal = total,
                    recordsFiltered = totalDisplay,
                    data = (from item in items
                            let discountFormatted = item.DiscountType == DiscountType.Percentage
                                ? $"{item.DiscountValue:0.##}%"
                                : $"{item.DiscountValue:N2} BDT"
                            let minSpendFormatted = item.MinimumSpend.HasValue
                                ? $"{item.MinimumSpend.Value:N2} BDT"
                                : "None"
                            let usageFormatted = item.UsageLimit.HasValue
                                ? $"{item.UsedCount} / {item.UsageLimit.Value}"
                                : $"{item.UsedCount} / ∞"
                            let statusText = !item.IsActive
                                ? "Inactive"
                                : (item.ExpirationDate < now ? "Expired" : "Active")
                            select new string[]
                            {
                                HttpUtility.HtmlEncode(item.Code),
                                discountFormatted,
                                minSpendFormatted,
                                usageFormatted,
                                item.ExpirationDate.ToString("MMM dd, yyyy"),
                                statusText,
                                item.Id.ToString()
                            }).ToArray()
                };

                return Json(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get paged coupons.");
                return Json(DataTables.EmptyResult);
            }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var command = new CouponToggleStatusCommand { Id = id };
                var isActive = await _mediator.SendCommandAsync(command, cancellationToken);

                TempData.Put(Constants.ResponseTempKey,
                    new ResponseModel
                    {
                        Message = $"Coupon marked as {(isActive ? "Active" : "Inactive")}.",
                        Type = ResponseTypes.Success
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to toggle coupon status for ID {Id}", id);
                TempData.Put(Constants.ResponseTempKey,
                    new ResponseModel
                    {
                        Message = "Failed to update coupon status.",
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
                var command = new CouponDeleteCommand { Id = id };
                await _mediator.SendCommandAsync(command, cancellationToken);

                TempData.Put(Constants.ResponseTempKey,
                    new ResponseModel
                    {
                        Message = "Coupon successfully deleted.",
                        Type = ResponseTypes.Success
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete coupon ID {Id}", id);
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
