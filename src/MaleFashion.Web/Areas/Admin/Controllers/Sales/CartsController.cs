using Cortex.Mediator;
using MaleFashion.Application.Features.Carts.Commands;
using MaleFashion.Application.Features.Carts.DTOs;
using MaleFashion.Application.Features.Carts.Queries;
using MaleFashion.Domain.Entities.Carts;
using MaleFashion.Domain.Utilities;
using MaleFashion.Web.Areas.Admin.Models.Carts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace MaleFashion.Web.Areas.Admin.Controllers.Sales
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class CartsController : Controller
    {
        private readonly IMediator _mediator;
        private readonly ILogger<CartsController> _logger;

        public CartsController(IMediator mediator, ILogger<CartsController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            try
            {
                var stats = await _mediator.SendQueryAsync<GetActiveCartStatsQuery, ActiveCartStatsDto>(
                    new GetActiveCartStatsQuery(), cancellationToken);

                ViewBag.Stats = stats;
                ViewBag.TotalActiveCarts = stats.TotalActiveCarts;
                ViewBag.PotentialRevenue = stats.PotentialRevenue;
                ViewBag.RegisteredCount = stats.RegisteredCartsCount;
                ViewBag.GuestCount = stats.GuestCartsCount;
                ViewBag.ActiveRecentCount = stats.ActiveRecentCount;
                ViewBag.IdleCount = stats.IdleCount;
                ViewBag.AbandonedCount = stats.AbandonedCount;
                ViewBag.RegisteredRevenue = stats.RegisteredRevenue;
                ViewBag.GuestRevenue = stats.GuestRevenue;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load active cart statistics.");
                ViewBag.Stats = new ActiveCartStatsDto();
                ViewBag.TotalActiveCarts = 0;
                ViewBag.PotentialRevenue = 0m;
                ViewBag.RegisteredCount = 0;
                ViewBag.GuestCount = 0;
                ViewBag.ActiveRecentCount = 0;
                ViewBag.IdleCount = 0;
                ViewBag.AbandonedCount = 0;
                ViewBag.RegisteredRevenue = 0m;
                ViewBag.GuestRevenue = 0m;
            }

            return View();
        }

        [HttpPost]
        public async Task<JsonResult> GetPagedCarts([FromBody] ActiveCartListModel model, CancellationToken cancellationToken)
        {
            try
            {
                var query = new GetAllActiveCartsByPagingQuery
                {
                    PageIndex = model.PageIndex,
                    PageSize = model.PageSize,
                    SearchText = model.Search.Value ?? string.Empty,
                    SortText = model.FormatSortExpression("CreatedAt", "UpdatedAt", "Id"),
                    CartFilter = model.CartFilter
                };

                var (items, total, totalDisplay) = await _mediator.SendQueryAsync<GetAllActiveCartsByPagingQuery,
                    (IList<Cart>, int, int)>(query, cancellationToken);

                var now = DateTime.UtcNow;
                var twoHoursAgo = now.AddHours(-2);
                var oneDayAgo = now.AddHours(-24);

                var response = new
                {
                    recordsTotal = total,
                    recordsFiltered = totalDisplay,
                    data = (from item in items
                            let lastActivity = item.UpdatedAt ?? item.CreatedAt
                            let isRegistered = item.Customer != null
                            let customerName = isRegistered ? $"{item.Customer!.FirstName} {item.Customer.LastName}".Trim() : "Guest Shopper"
                            let customerEmail = isRegistered ? item.Customer!.Email : "N/A (Anonymous)"
                            let customerPhone = isRegistered ? (item.Customer!.PhoneNumber ?? string.Empty) : string.Empty
                            let avatarInitials = isRegistered && !string.IsNullOrWhiteSpace(customerName)
                                ? (customerName.Length >= 2 ? customerName.Substring(0, 2).ToUpper() : customerName.ToUpper())
                                : "GS"
                            let customerBadge = isRegistered
                                ? "<span class='badge bg-label-primary ms-1'><i class='bx bx-user-check'></i> Registered</span>"
                                : "<span class='badge bg-label-secondary ms-1'><i class='bx bx-user'></i> Guest</span>"
                            let customerHtml = $@"<div class='d-flex align-items-center'>
                                <div class='avatar avatar-sm me-2'>
                                    <span class='avatar-initial rounded-circle bg-label-info'>{HttpUtility.HtmlEncode(avatarInitials)}</span>
                                </div>
                                <div>
                                    <span class='fw-semibold d-block'>{HttpUtility.HtmlEncode(customerName)} {customerBadge}</span>
                                    <small class='text-muted d-block'>{HttpUtility.HtmlEncode(customerEmail)}</small>
                                    {(!string.IsNullOrEmpty(customerPhone) ? $"<small class='text-muted d-block'><i class='bx bx-phone'></i> {HttpUtility.HtmlEncode(customerPhone)}</small>" : "")}
                                    {(!isRegistered ? $"<small class='text-muted font-monospace'>Session: {HttpUtility.HtmlEncode(item.SessionId.Length > 12 ? item.SessionId.Substring(0, 12) + "..." : item.SessionId)}</small>" : "")}
                                </div>
                            </div>"
                            let itemCount = item.Items.Count
                            let totalQty = item.Items.Sum(i => i.Quantity)
                            let totalVal = item.Items.Sum(i => i.Quantity * i.UnitPrice.Amount)
                            let currency = item.Items.FirstOrDefault()?.UnitPrice.Currency ?? "BDT"
                            let thumbs = string.Join("", item.Items.Take(3).Select(i =>
                            {
                                var img = !string.IsNullOrEmpty(i.ImageUrl) ? $"/uploads/products/{HttpUtility.HtmlEncode(i.ImageUrl)}" : "/img/product/product-1.jpg";
                                return $"<img src='{img}' class='rounded me-1 border' style='width:32px;height:32px;object-fit:cover;' title='{HttpUtility.HtmlEncode(i.ProductName)} (x{i.Quantity})' />";
                            }))
                            let extraThumbs = itemCount > 3 ? $"<span class='badge bg-label-secondary small'>+{itemCount - 3}</span>" : ""
                            let itemsHtml = $@"<div>
                                <div class='mb-1'>{thumbs}{extraThumbs}</div>
                                <span class='fw-semibold'>{itemCount} item(s)</span> <span class='text-muted small'>({totalQty} pcs)</span>
                            </div>"
                            let couponHtml = !string.IsNullOrEmpty(item.AppliedCouponCode)
                                ? $"<div class='mt-1'><span class='badge bg-label-success'><i class='bx bx-purchase-tag me-1'></i>{HttpUtility.HtmlEncode(item.AppliedCouponCode)}</span></div>"
                                : ""
                            let valueHtml = $@"<div>
                                <span class='fw-bold text-primary fs-6'>{totalVal:N2} {currency}</span>
                                {couponHtml}
                            </div>"
                            let statusText = lastActivity >= twoHoursAgo
                                ? "<span class='badge bg-label-success'><i class='bx bx-pulse me-1'></i>Active</span>"
                                : (lastActivity >= oneDayAgo
                                    ? "<span class='badge bg-label-warning'><i class='bx bx-time me-1'></i>Idle</span>"
                                    : "<span class='badge bg-label-danger'><i class='bx bx-alarm-exclamation me-1'></i>Abandoned</span>")
                            let dateHtml = $@"<div>
                                <span class='d-block'>{lastActivity.ToString("MMM dd, yyyy HH:mm")}</span>
                                <small class='text-muted'>{GetRelativeTime(lastActivity, now)}</small>
                            </div>"
                            select new string[]
                            {
                                customerHtml,
                                itemsHtml,
                                valueHtml,
                                statusText,
                                dateHtml,
                                item.Id.ToString()
                            }).ToArray()
                };

                return Json(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get active carts list.");
                return Json(DataTables.EmptyResult);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetDetails(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var cart = await _mediator.SendQueryAsync<GetActiveCartByIdQuery, Cart?>(
                    new GetActiveCartByIdQuery(id), cancellationToken);

                if (cart == null)
                    return Json(new { success = false, message = "Cart not found." });

                var isRegistered = cart.Customer != null;
                var now = DateTime.UtcNow;
                var lastActivity = cart.UpdatedAt ?? cart.CreatedAt;
                var statusLabel = lastActivity >= now.AddHours(-2)
                    ? "Active"
                    : (lastActivity >= now.AddHours(-24) ? "Idle" : "Abandoned");

                var primaryAddress = cart.Customer?.Addresses?.FirstOrDefault(a => a.IsDefault)?.Address
                    ?? cart.Customer?.Addresses?.FirstOrDefault()?.Address;
                string? addressFormatted = null;
                if (primaryAddress != null)
                {
                    addressFormatted = $"{primaryAddress.StreetAddress}, {primaryAddress.City}, {primaryAddress.PostalCode}, {primaryAddress.Country}".Trim(',', ' ');
                }

                var model = new ActiveCartViewModel
                {
                    Id = cart.Id,
                    CustomerId = cart.CustomerId,
                    CustomerName = isRegistered ? $"{cart.Customer!.FirstName} {cart.Customer.LastName}".Trim() : "Guest Shopper",
                    CustomerEmail = isRegistered ? cart.Customer!.Email : string.Empty,
                    CustomerPhone = isRegistered ? (cart.Customer!.PhoneNumber ?? string.Empty) : string.Empty,
                    CustomerAddress = addressFormatted,
                    IsRegistered = isRegistered,
                    SessionId = cart.SessionId,
                    AppliedCouponCode = cart.AppliedCouponCode,
                    CreatedAt = cart.CreatedAt,
                    UpdatedAt = cart.UpdatedAt,
                    TotalValue = cart.Items.Sum(i => i.Quantity * i.UnitPrice.Amount),
                    TotalItemsCount = cart.Items.Sum(i => i.Quantity),
                    StatusLabel = statusLabel,
                    Items = cart.Items.Select(i =>
                    {
                        var variantInfo = i.ProductVariant != null
                            ? $"{i.ProductVariant.ColorName} {i.ProductVariant.Size}".Trim()
                            : null;

                        var img = !string.IsNullOrEmpty(i.ImageUrl)
                            ? $"/uploads/products/{i.ImageUrl}"
                            : (i.Product?.Images?.FirstOrDefault()?.ImageUrl != null ? $"/uploads/products/{i.Product.Images.First().ImageUrl}" : "/img/product/product-1.jpg");

                        return new CartItemViewModel
                        {
                            ProductId = i.ProductId,
                            ProductName = i.ProductName,
                            VariantDetails = variantInfo,
                            ImageUrl = img,
                            Quantity = i.Quantity,
                            UnitPrice = i.UnitPrice.Amount,
                            LineTotal = i.Quantity * i.UnitPrice.Amount
                        };
                    }).ToList()
                };

                return Json(new { success = true, cart = model });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get cart details for ID {CartId}", id);
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                await _mediator.SendCommandAsync(new CartDeleteCommand(id), cancellationToken);
                return Json(new { success = true, message = "Cart removed successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete cart with ID {CartId}", id);
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ClearStale(int days = 30, CancellationToken cancellationToken = default)
        {
            try
            {
                var count = await _mediator.SendCommandAsync(new CartClearStaleCommand(days), cancellationToken);
                return Json(new { success = true, message = $"Cleaned up {count} stale carts older than {days} days." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to clear stale carts.");
                return Json(new { success = false, message = ex.Message });
            }
        }

        private static string GetRelativeTime(DateTime pastDate, DateTime now)
        {
            var span = now - pastDate;
            if (span.TotalMinutes < 2) return "just now";
            if (span.TotalMinutes < 60) return $"{(int)span.TotalMinutes} mins ago";
            if (span.TotalHours < 24) return $"{(int)span.TotalHours} hours ago";
            if (span.TotalDays < 30) return $"{(int)span.TotalDays} days ago";
            return $"{(int)(span.TotalDays / 30)} months ago";
        }
    }
}

