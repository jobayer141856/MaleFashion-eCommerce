using Cortex.Mediator;
using MaleFashion.Application.Features.Ordering.Commands;
using MaleFashion.Application.Features.Ordering.DTOs;
using MaleFashion.Application.Features.Ordering.Queries;
using MaleFashion.Domain.Entities.Ordering;
using MaleFashion.Domain.Enums;
using MaleFashion.Domain.Utilities;
using MaleFashion.Domain.ValueOfObjects;
using MaleFashion.Web.Areas.Admin.Models.Ordering;
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

namespace MaleFashion.Web.Areas.Admin.Controllers.Ordering
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class OrdersController : Controller
    {
        private readonly IMediator _mediator;
        private readonly ILogger<OrdersController> _logger;

        public OrdersController(IMediator mediator, ILogger<OrdersController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            try
            {
                var stats = await _mediator.SendQueryAsync<GetOrderStatsQuery, OrderStatsDto>(
                    new GetOrderStatsQuery(), cancellationToken);

                ViewBag.Stats = stats;
                ViewBag.TotalOrders = stats.TotalOrders;
                ViewBag.PendingOrders = stats.PendingOrders;
                ViewBag.ProcessingOrders = stats.ProcessingOrders;
                ViewBag.ShippedOrders = stats.ShippedOrders;
                ViewBag.DeliveredOrders = stats.DeliveredOrders;
                ViewBag.CancelledOrders = stats.CancelledOrders;
                ViewBag.TotalSales = stats.TotalSales;
                ViewBag.CashSales = stats.CashSales;
                ViewBag.StripeSales = stats.StripeSales;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load order statistics.");
                ViewBag.Stats = new OrderStatsDto();
                ViewBag.TotalOrders = 0;
                ViewBag.PendingOrders = 0;
                ViewBag.ProcessingOrders = 0;
                ViewBag.ShippedOrders = 0;
                ViewBag.DeliveredOrders = 0;
                ViewBag.CancelledOrders = 0;
                ViewBag.TotalSales = 0m;
                ViewBag.CashSales = 0m;
                ViewBag.StripeSales = 0m;
            }

            return View();
        }

        [HttpPost]
        public async Task<JsonResult> GetPagedOrders([FromBody] OrderListModel model, CancellationToken cancellationToken)
        {
            try
            {
                var query = new GetAllOrdersByPagingQuery
                {
                    PageIndex = model.PageIndex,
                    PageSize = model.PageSize,
                    SearchText = model.Search.Value ?? string.Empty,
                    SortText = model.FormatSortExpression("OrderNumber", "Customer.FirstName", "OrderDate", "Id", "TotalAmount.Amount", "PaymentStatus", "Status", "Id"),
                    StatusFilter = model.StatusFilter,
                    PaymentStatusFilter = model.PaymentStatusFilter,
                    PaymentMethodFilter = model.PaymentMethodFilter
                };

                var (items, total, totalDisplay) = await _mediator.SendQueryAsync<GetAllOrdersByPagingQuery,
                    (IList<Order>, int, int)>(query, cancellationToken);

                var now = DateTime.UtcNow;

                var response = new
                {
                    recordsTotal = total,
                    recordsFiltered = totalDisplay,
                    data = (from item in items
                            let isRegistered = item.Customer != null
                            let customerName = isRegistered ? $"{item.Customer!.FirstName} {item.Customer.LastName}".Trim() : "Guest Customer"
                            let customerEmail = isRegistered ? item.Customer!.Email : "N/A"
                            let customerPhone = isRegistered ? (item.Customer!.PhoneNumber ?? string.Empty) : string.Empty
                            let avatarInitials = isRegistered && !string.IsNullOrWhiteSpace(customerName)
                                ? (customerName.Length >= 2 ? customerName.Substring(0, 2).ToUpper() : customerName.ToUpper())
                                : "GC"
                            let customerBadge = isRegistered
                                ? "<span class='badge bg-label-primary ms-1'>Registered</span>"
                                : "<span class='badge bg-label-secondary ms-1'>Guest</span>"
                            let customerHtml = $@"<div class='d-flex align-items-center'>
                                <div class='avatar avatar-sm me-2'>
                                    <span class='avatar-initial rounded-circle bg-label-primary'>{HttpUtility.HtmlEncode(avatarInitials)}</span>
                                </div>
                                <div>
                                    <span class='fw-semibold d-block'>{HttpUtility.HtmlEncode(customerName)} {customerBadge}</span>
                                    <small class='text-muted d-block'>{HttpUtility.HtmlEncode(customerEmail)}</small>
                                    {(!string.IsNullOrEmpty(customerPhone) ? $"<small class='text-muted d-block'><i class='bx bx-phone'></i> {HttpUtility.HtmlEncode(customerPhone)}</small>" : "")}
                                </div>
                            </div>"
                            let orderNumHtml = $@"<a href='/Admin/Orders/Details/{item.Id}' class='fw-bold text-primary'>
                                <i class='bx bx-purchase-tag-alt me-1'></i>#{HttpUtility.HtmlEncode(item.OrderNumber)}
                            </a>"
                            let dateHtml = $@"<div>
                                <span class='d-block'>{item.OrderDate.ToString("MMM dd, yyyy HH:mm")}</span>
                                <small class='text-muted'>{GetRelativeTime(item.OrderDate, now)}</small>
                            </div>"
                            let itemCount = item.OrderItems.Count
                            let totalQty = item.OrderItems.Sum(i => i.Quantity)
                            let itemsHtml = $"<div><span class='fw-semibold'>{itemCount} item(s)</span> <small class='text-muted'>({totalQty} pcs)</small></div>"
                            let totalVal = item.TotalAmount.Amount
                            let currency = item.TotalAmount.Currency
                            let totalHtml = $"<span class='fw-bold text-dark fs-6'>{totalVal:N2} {currency}</span>"
                            let paymentBadge = GetPaymentStatusBadge(item.PaymentStatus)
                            let paymentMethodIcon = item.PaymentMethod == PaymentMethod.Cash ? "<i class='bx bx-money text-success me-1'></i>Cash" : "<i class='bx bxl-stripe text-primary me-1'></i>Stripe"
                            let paymentHtml = $@"<div>
                                <div>{paymentMethodIcon}</div>
                                <div class='mt-1'>{paymentBadge}</div>
                            </div>"
                            let statusBadge = GetOrderStatusBadge(item.Status)
                            select new string[]
                            {
                                orderNumHtml,
                                customerHtml,
                                dateHtml,
                                itemsHtml,
                                totalHtml,
                                paymentHtml,
                                statusBadge,
                                item.Id.ToString()
                            }).ToArray()
                };

                return Json(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get paged orders.");
                return Json(DataTables.EmptyResult);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
        {
            var order = await _mediator.SendQueryAsync<GetOrderByIdQuery, Order?>(
                new GetOrderByIdQuery(id), cancellationToken);

            if (order == null)
                return NotFound();

            var model = MapToDetailsViewModel(order);
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Invoice(Guid id, CancellationToken cancellationToken)
        {
            var order = await _mediator.SendQueryAsync<GetOrderByIdQuery, Order?>(
                new GetOrderByIdQuery(id), cancellationToken);

            if (order == null)
                return NotFound();

            var model = MapToDetailsViewModel(order);
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStatus(Guid id, OrderStatus status, CancellationToken cancellationToken)
        {
            try
            {
                await _mediator.SendCommandAsync(new OrderUpdateStatusCommand(id, status), cancellationToken);
                return Json(new { success = true, message = $"Order status updated to {status}." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update status for order ID {OrderId}", id);
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdatePaymentStatus(Guid id, PaymentStatus paymentStatus, string? transactionId, CancellationToken cancellationToken)
        {
            try
            {
                await _mediator.SendCommandAsync(new OrderUpdatePaymentStatusCommand(id, paymentStatus, transactionId), cancellationToken);
                return Json(new { success = true, message = $"Payment status updated to {paymentStatus}." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update payment status for order ID {OrderId}", id);
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Cancel(Guid id, string? reason, CancellationToken cancellationToken)
        {
            try
            {
                await _mediator.SendCommandAsync(new OrderCancelCommand(id, reason), cancellationToken);
                return Json(new { success = true, message = "Order cancelled successfully and stock restored." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to cancel order ID {OrderId}", id);
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                await _mediator.SendCommandAsync(new OrderDeleteCommand(id), cancellationToken);
                return Json(new { success = true, message = "Order deleted successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete order ID {OrderId}", id);
                return Json(new { success = false, message = ex.Message });
            }
        }

        private static OrderDetailsViewModel MapToDetailsViewModel(Order order)
        {
            var isRegistered = order.Customer != null;
            var customerName = isRegistered ? $"{order.Customer!.FirstName} {order.Customer.LastName}".Trim() : "Guest Customer";
            var customerEmail = isRegistered ? order.Customer!.Email : "N/A";
            var customerPhone = isRegistered ? (order.Customer!.PhoneNumber ?? string.Empty) : string.Empty;

            var shippingAddress = FormatAddress(order.ShippingAddress);
            var billingAddress = FormatAddress(order.BillingAddress);

            return new OrderDetailsViewModel
            {
                Id = order.Id,
                OrderNumber = order.OrderNumber,
                OrderDate = order.OrderDate,
                Status = order.Status,
                PaymentStatus = order.PaymentStatus,
                PaymentMethod = order.PaymentMethod,
                TransactionId = order.TransactionId,
                OrderNotes = order.OrderNotes,
                CustomerId = order.CustomerId,
                CustomerName = customerName,
                CustomerEmail = customerEmail,
                CustomerPhone = customerPhone,
                IsGuest = !isRegistered,
                ShippingAddressFormatted = shippingAddress,
                BillingAddressFormatted = billingAddress,
                Subtotal = order.Subtotal.Amount,
                ShippingCost = order.ShippingCost?.Amount ?? 0m,
                DiscountAmount = order.DiscountAmount?.Amount ?? 0m,
                TotalAmount = order.TotalAmount.Amount,
                Currency = order.TotalAmount.Currency,
                CouponCode = order.Coupon?.Code,
                CreatedAt = order.CreatedAt,
                UpdatedAt = order.UpdatedAt,
                Items = order.OrderItems.Select(i =>
                {
                    var variantInfo = i.ProductVariant != null
                        ? $"{i.ProductVariant.ColorName} {i.ProductVariant.Size}".Trim()
                        : null;

                    var img = i.Product?.Images?.FirstOrDefault()?.ImageUrl;
                    var imgUrl = !string.IsNullOrEmpty(img) ? $"/uploads/products/{img}" : "/img/product/product-1.jpg";

                    return new OrderItemViewModel
                    {
                        ProductId = i.ProductId,
                        ProductName = i.ProductName,
                        VariantInfo = variantInfo,
                        ImageUrl = imgUrl,
                        Quantity = i.Quantity,
                        UnitPrice = i.UnitPrice.Amount,
                        TotalPrice = i.TotalPrice.Amount,
                        Currency = i.UnitPrice.Currency
                    };
                }).ToList()
            };
        }

        private static string FormatAddress(Address? addr)
        {
            if (addr == null) return "N/A";
            var parts = new List<string>();
            if (!string.IsNullOrWhiteSpace(addr.StreetAddress)) parts.Add(addr.StreetAddress);
            if (!string.IsNullOrWhiteSpace(addr.ApartmentSuite)) parts.Add(addr.ApartmentSuite);
            if (!string.IsNullOrWhiteSpace(addr.City)) parts.Add(addr.City);
            if (!string.IsNullOrWhiteSpace(addr.State)) parts.Add(addr.State);
            if (!string.IsNullOrWhiteSpace(addr.PostalCode)) parts.Add(addr.PostalCode);
            if (!string.IsNullOrWhiteSpace(addr.Country)) parts.Add(addr.Country);
            return string.Join(", ", parts);
        }

        private static string GetOrderStatusBadge(OrderStatus status)
        {
            return status switch
            {
                OrderStatus.Pending => "<span class='badge bg-label-warning'><i class='bx bx-time-five me-1'></i>Pending</span>",
                OrderStatus.Processing => "<span class='badge bg-label-info'><i class='bx bx-sync me-1'></i>Processing</span>",
                OrderStatus.Shipped => "<span class='badge bg-label-primary'><i class='bx bx-package me-1'></i>Shipped</span>",
                OrderStatus.Delivered => "<span class='badge bg-label-success'><i class='bx bx-check-double me-1'></i>Delivered</span>",
                OrderStatus.Cancelled => "<span class='badge bg-label-danger'><i class='bx bx-x-circle me-1'></i>Cancelled</span>",
                _ => $"<span class='badge bg-label-secondary'>{status}</span>"
            };
        }

        private static string GetPaymentStatusBadge(PaymentStatus status)
        {
            return status switch
            {
                PaymentStatus.Paid => "<span class='badge bg-label-success'><i class='bx bx-check me-1'></i>Paid</span>",
                PaymentStatus.Pending => "<span class='badge bg-label-warning'><i class='bx bx-hourglass me-1'></i>Pending</span>",
                PaymentStatus.Failed => "<span class='badge bg-label-danger'><i class='bx bx-error me-1'></i>Failed</span>",
                PaymentStatus.Refunded => "<span class='badge bg-label-info'><i class='bx bx-redo me-1'></i>Refunded</span>",
                _ => $"<span class='badge bg-label-secondary'>{status}</span>"
            };
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

