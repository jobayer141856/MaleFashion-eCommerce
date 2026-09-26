using Cortex.Mediator;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Contracts.Services;
using MaleFashion.Application.Features.Carts.DTOs;
using MaleFashion.Application.Features.Carts.Queries;
using MaleFashion.Application.Features.Ordering.Commands;
using MaleFashion.Application.Features.Ordering.DTOs;
using MaleFashion.Application.Features.Ordering.Queries;
using MaleFashion.Domain.Entities.Ordering;
using MaleFashion.Domain.Enums;
using MaleFashion.Domain.ValueOfObjects;
using MaleFashion.Infrastructure.Identity;
using MaleFashion.Web.Models.Checkout;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Web.Controllers.Ordering
{
    public class CheckoutController : Controller
    {
        private readonly IMediator _mediator;
        private readonly ICartSessionService _cartSessionService;
        private readonly IApplicationUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ISSLCommerzService _sslCommerzService;
        private readonly IEmailService _emailService;
        private readonly ILogger<CheckoutController> _logger;

        public CheckoutController(
            IMediator mediator,
            ICartSessionService cartSessionService,
            IApplicationUnitOfWork unitOfWork,
            UserManager<ApplicationUser> userManager,
            ISSLCommerzService sslCommerzService,
            IEmailService emailService,
            ILogger<CheckoutController> logger)
        {
            _mediator = mediator;
            _cartSessionService = cartSessionService;
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _sslCommerzService = sslCommerzService;
            _emailService = emailService;
            _logger = logger;
        }

        // GET: /Checkout
        [HttpGet]
        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            var (customerId, sessionId) = await _cartSessionService.GetCartIdentityAsync(cancellationToken);
            var cart = await _mediator.SendQueryAsync<GetCustomerCartQuery, CustomerCartDto>(
                new GetCustomerCartQuery { CustomerId = customerId, SessionId = sessionId }, cancellationToken);

            if (cart == null || !cart.Items.Any())
            {
                TempData["CartWarning"] = "Your shopping cart is currently empty. Please add products before checking out.";
                return RedirectToAction("Index", "Cart");
            }

            var model = new CheckoutViewModel
            {
                Cart = cart,
                PaymentMethod = PaymentMethod.SSLCommerz
            };

            // Pre-fill profile and address if authenticated
            if (User.Identity?.IsAuthenticated == true)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    model.FirstName = user.FirstName ?? string.Empty;
                    model.LastName = user.LastName ?? string.Empty;
                    model.Email = user.Email ?? string.Empty;
                    model.PhoneNumber = user.PhoneNumber ?? string.Empty;

                    var customer = await _unitOfWork.CustomerRepository.GetCustomerByUserIdAsync(user.Id, cancellationToken);
                    if (customer != null && customer.Addresses != null && customer.Addresses.Any())
                    {
                        model.SavedAddresses = customer.Addresses.ToList();
                        var defaultAddress = customer.Addresses.FirstOrDefault(a => a.IsDefault) ?? customer.Addresses.First();
                        if (defaultAddress.Address != null)
                        {
                            model.ShippingStreetAddress = defaultAddress.Address.StreetAddress;
                            model.ShippingApartmentSuite = defaultAddress.Address.ApartmentSuite;
                            model.ShippingCity = defaultAddress.Address.City;
                            model.ShippingState = defaultAddress.Address.State;
                            model.ShippingPostalCode = defaultAddress.Address.PostalCode;
                            model.ShippingCountry = defaultAddress.Address.Country;
                        }
                    }
                }
            }

            return View(model);
        }

        // POST: /Checkout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(CheckoutViewModel model, CancellationToken cancellationToken)
        {
            var (customerId, sessionId) = await _cartSessionService.GetCartIdentityAsync(cancellationToken);
            var cart = await _mediator.SendQueryAsync<GetCustomerCartQuery, CustomerCartDto>(
                new GetCustomerCartQuery { CustomerId = customerId, SessionId = sessionId }, cancellationToken);

            if (cart == null || !cart.Items.Any())
            {
                TempData["CartWarning"] = "Your shopping cart is empty.";
                return RedirectToAction("Index", "Cart");
            }

            if (!ModelState.IsValid)
            {
                model.Cart = cart;
                return View(model);
            }

            var shippingAddress = new Address(
                model.ShippingStreetAddress,
                model.ShippingApartmentSuite,
                model.ShippingCity,
                model.ShippingState,
                model.ShippingPostalCode,
                model.ShippingCountry);

            var billingAddress = model.ShipToDifferentAddress && !string.IsNullOrWhiteSpace(model.BillingStreetAddress)
                ? new Address(
                    model.BillingStreetAddress,
                    model.BillingApartmentSuite,
                    model.BillingCity ?? model.ShippingCity,
                    model.BillingState ?? model.ShippingState,
                    model.BillingPostalCode ?? model.ShippingPostalCode,
                    model.BillingCountry ?? model.ShippingCountry)
                : (shippingAddress with { });

            var command = new CreateOrderCommand
            {
                CustomerId = customerId,
                SessionId = sessionId,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                ShippingAddress = shippingAddress,
                BillingAddress = billingAddress,
                PaymentMethod = model.PaymentMethod,
                OrderNotes = model.OrderNotes
            };

            var result = await _mediator.SendCommandAsync<CreateOrderCommand, CreateOrderResultDto>(command, cancellationToken);

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                model.Cart = cart;
                return View(model);
            }

            // Route based on payment method
            if (result.PaymentMethod == PaymentMethod.Cash)
            {
                await SendOrderConfirmationEmailAsync($"{model.FirstName} {model.LastName}".Trim(), model.Email, result.OrderNumber, result.TotalAmount, "Cash on Delivery");
                TempData["OrderSuccess"] = "Your order has been placed successfully! Payment will be collected on delivery.";
                return RedirectToAction(nameof(Confirmation), new { orderNumber = result.OrderNumber });
            }

            if (result.PaymentMethod == PaymentMethod.SSLCommerz)
            {
                var domain = $"{Request.Scheme}://{Request.Host}";
                var sslRequest = new SSLCommerzInitRequest
                {
                    TotalAmount = result.TotalAmount,
                    Currency = "BDT",
                    TransactionId = result.OrderNumber,
                    SuccessUrl = $"{domain}/Checkout/PaymentSuccess",
                    FailUrl = $"{domain}/Checkout/PaymentFail",
                    CancelUrl = $"{domain}/Checkout/PaymentCancel",
                    IpnUrl = $"{domain}/Checkout/PaymentIpn",
                    CustomerName = $"{model.FirstName} {model.LastName}".Trim(),
                    CustomerEmail = model.Email,
                    CustomerAddress = model.ShippingStreetAddress,
                    CustomerCity = model.ShippingCity,
                    CustomerPostCode = model.ShippingPostalCode,
                    CustomerCountry = model.ShippingCountry,
                    CustomerPhone = model.PhoneNumber,
                    ShippingName = $"{model.FirstName} {model.LastName}".Trim(),
                    ShippingAddress = model.ShippingStreetAddress,
                    ShippingCity = model.ShippingCity,
                    ShippingPostCode = model.ShippingPostalCode,
                    ShippingCountry = model.ShippingCountry,
                    TotalItems = cart.TotalItemCount
                };

                var sslResponse = await _sslCommerzService.InitiatePaymentAsync(sslRequest, cancellationToken);
                if (sslResponse.IsSuccess && !string.IsNullOrWhiteSpace(sslResponse.GatewayPageURL))
                {
                    return Redirect(sslResponse.GatewayPageURL);
                }

                _logger.LogError("SSLCommerz initiation failed for order {OrderNumber}: {Reason}", result.OrderNumber, sslResponse.FailedReason);
                TempData["OrderWarning"] = $"Order placed with number {result.OrderNumber}, but unable to initiate online payment ({sslResponse.FailedReason ?? "Gateway unavailable"}). Please contact support or pay on delivery.";
                return RedirectToAction(nameof(Confirmation), new { orderNumber = result.OrderNumber });
            }

            return RedirectToAction(nameof(Confirmation), new { orderNumber = result.OrderNumber });
        }

        // POST: /Checkout/PaymentSuccess
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> PaymentSuccess(
            [FromForm] string? tran_id,
            [FromForm] string? val_id,
            [FromForm] string? amount,
            [FromForm] string? bank_tran_id,
            [FromForm] string? card_type,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("SSLCommerz PaymentSuccess callback received: TranId={TranId}, ValId={ValId}", tran_id, val_id);

            if (string.IsNullOrWhiteSpace(tran_id))
            {
                return RedirectToAction("Index", "Home");
            }

            if (!string.IsNullOrWhiteSpace(val_id))
            {
                var validation = await _sslCommerzService.ValidatePaymentAsync(val_id, cancellationToken);
                if (validation.IsValid)
                {
                    await _mediator.SendCommandAsync<OrderUpdatePaymentResultCommand, bool>(new OrderUpdatePaymentResultCommand
                    {
                        OrderNumber = tran_id,
                        PaymentStatus = PaymentStatus.Paid,
                        TransactionId = validation.BankTranId ?? val_id,
                        GatewayNotes = $"SSLCommerz Payment Validated: Card={validation.CardType ?? card_type}, BankTranId={validation.BankTranId ?? bank_tran_id}, ValId={val_id}"
                    }, cancellationToken);

                    var order = await _unitOfWork.OrderRepository.GetOrderByOrderNumberWithDetailsAsync(tran_id, cancellationToken);
                    if (order != null && order.Customer != null && !string.IsNullOrWhiteSpace(order.Customer.Email))
                    {
                        await SendOrderConfirmationEmailAsync($"{order.Customer.FirstName} {order.Customer.LastName}".Trim(), order.Customer.Email, order.OrderNumber, order.TotalAmount.Amount, "SSLCommerz");
                    }

                    TempData["OrderSuccess"] = "Payment successful! Your order is being processed.";
                    return RedirectToAction(nameof(Confirmation), new { orderNumber = tran_id });
                }
            }

            TempData["OrderWarning"] = "Payment verification is pending. Our support team will confirm your order shortly.";
            return RedirectToAction(nameof(Confirmation), new { orderNumber = tran_id });
        }

        // POST: /Checkout/PaymentFail
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> PaymentFail(
            [FromForm] string? tran_id,
            [FromForm] string? failedreason,
            CancellationToken cancellationToken)
        {
            _logger.LogWarning("SSLCommerz PaymentFail: TranId={TranId}, Reason={Reason}", tran_id, failedreason);

            if (!string.IsNullOrWhiteSpace(tran_id))
            {
                await _mediator.SendCommandAsync<OrderUpdatePaymentResultCommand, bool>(new OrderUpdatePaymentResultCommand
                {
                    OrderNumber = tran_id,
                    PaymentStatus = PaymentStatus.Failed,
                    GatewayNotes = $"SSLCommerz Payment Failed: {failedreason}"
                }, cancellationToken);

                TempData["OrderError"] = $"Online payment was not successful ({failedreason ?? "Transaction failed"}). Your order is pending.";
                return RedirectToAction(nameof(Confirmation), new { orderNumber = tran_id });
            }

            return RedirectToAction("Index", "Home");
        }

        // POST: /Checkout/PaymentCancel
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> PaymentCancel(
            [FromForm] string? tran_id,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("SSLCommerz PaymentCancel: TranId={TranId}", tran_id);

            if (!string.IsNullOrWhiteSpace(tran_id))
            {
                await _mediator.SendCommandAsync<OrderUpdatePaymentResultCommand, bool>(new OrderUpdatePaymentResultCommand
                {
                    OrderNumber = tran_id,
                    PaymentStatus = PaymentStatus.Pending,
                    GatewayNotes = "SSLCommerz Payment cancelled by user."
                }, cancellationToken);

                TempData["OrderWarning"] = "Online payment was cancelled. Your order remains pending.";
                return RedirectToAction(nameof(Confirmation), new { orderNumber = tran_id });
            }

            return RedirectToAction("Index", "Cart");
        }

        // POST: /Checkout/PaymentIpn
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> PaymentIpn(
            [FromForm] string? tran_id,
            [FromForm] string? val_id,
            [FromForm] string? status,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("SSLCommerz IPN Notification received: TranId={TranId}, ValId={ValId}, Status={Status}", tran_id, val_id, status);

            if (!string.IsNullOrWhiteSpace(tran_id) && !string.IsNullOrWhiteSpace(val_id))
            {
                var validation = await _sslCommerzService.ValidatePaymentAsync(val_id, cancellationToken);
                if (validation.IsValid)
                {
                    await _mediator.SendCommandAsync<OrderUpdatePaymentResultCommand, bool>(new OrderUpdatePaymentResultCommand
                    {
                        OrderNumber = tran_id,
                        PaymentStatus = PaymentStatus.Paid,
                        TransactionId = validation.BankTranId ?? val_id,
                        GatewayNotes = $"SSLCommerz IPN Confirmed: BankTranId={validation.BankTranId}, ValId={val_id}"
                    }, cancellationToken);
                }
            }

            return Ok("IPN Handled");
        }

        // GET: /Checkout/Confirmation
        [HttpGet]
        public async Task<IActionResult> Confirmation(string orderNumber, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(orderNumber))
            {
                return RedirectToAction("Index", "Home");
            }

            var order = await _mediator.SendQueryAsync<GetOrderByOrderNumberQuery, Order?>(
                new GetOrderByOrderNumberQuery { OrderNumber = orderNumber }, cancellationToken);

            if (order == null)
            {
                return NotFound("Order not found.");
            }

            var shippingAddr = order.ShippingAddress != null
                ? $"{order.ShippingAddress.StreetAddress}, {order.ShippingAddress.City} - {order.ShippingAddress.PostalCode}, {order.ShippingAddress.Country}"
                : "Standard Delivery Address";

            var billingAddr = order.BillingAddress != null
                ? $"{order.BillingAddress.StreetAddress}, {order.BillingAddress.City} - {order.BillingAddress.PostalCode}, {order.BillingAddress.Country}"
                : shippingAddr;

            var model = new OrderConfirmationViewModel
            {
                OrderId = order.Id,
                OrderNumber = order.OrderNumber,
                OrderDate = order.OrderDate,
                Status = order.Status,
                PaymentStatus = order.PaymentStatus,
                PaymentMethod = order.PaymentMethod,
                TransactionId = order.TransactionId,
                Subtotal = order.Subtotal?.Amount ?? 0,
                ShippingCost = order.ShippingCost?.Amount ?? 0,
                DiscountAmount = order.DiscountAmount?.Amount ?? 0,
                TotalAmount = order.TotalAmount?.Amount ?? 0,
                CustomerName = order.Customer != null ? $"{order.Customer.FirstName} {order.Customer.LastName}".Trim() : "Valued Customer",
                CustomerEmail = order.Customer?.Email ?? string.Empty,
                CustomerPhone = order.Customer?.PhoneNumber ?? string.Empty,
                ShippingAddressText = shippingAddr,
                BillingAddressText = billingAddr,
                OrderNotes = order.OrderNotes,
                Items = order.OrderItems.Select(i => new OrderConfirmationItemViewModel
                {
                    ProductId = i.ProductId,
                    ProductName = i.ProductName,
                    Size = i.ProductVariant?.Size,
                    Color = i.ProductVariant?.ColorName,
                    ImageUrl = i.Product?.Images?.FirstOrDefault(img => img.IsPrimary)?.ImageUrl 
                               ?? i.Product?.Images?.FirstOrDefault()?.ImageUrl 
                               ?? "/malefashion/img/product/product-default.jpg",
                    UnitPrice = i.UnitPrice?.Amount ?? 0,
                    Quantity = i.Quantity,
                    TotalPrice = i.TotalPrice?.Amount ?? 0
                }).ToList()
            };

            return View(model);
        }

        private async Task SendOrderConfirmationEmailAsync(string customerName, string customerEmail, string orderNumber, decimal totalAmount, string paymentMethod)
        {
            try
            {
                var subject = $"Order Confirmation - {orderNumber} | MaleFashion";
                var trackUrl = $"{Request.Scheme}://{Request.Host}/Order/Track?orderNumber={orderNumber}";
                var body = $@"
                    <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; border: 1px solid #e1e1e1; padding: 25px; border-radius: 8px;'>
                        <h2 style='color: #e53637; margin-bottom: 5px;'>MaleFashion</h2>
                        <h3 style='margin-top: 0;'>Thank you for your order!</h3>
                        <p>Dear <strong>{customerName}</strong>,</p>
                        <p>We are pleased to confirm that your order <strong>{orderNumber}</strong> has been received and is being processed.</p>
                        
                        <table style='width: 100%; border-collapse: collapse; margin: 20px 0;'>
                            <tr style='background: #f8f9fa;'>
                                <td style='padding: 10px; border: 1px solid #dee2e6;'><strong>Order Number:</strong></td>
                                <td style='padding: 10px; border: 1px solid #dee2e6;'>{orderNumber}</td>
                            </tr>
                            <tr>
                                <td style='padding: 10px; border: 1px solid #dee2e6;'><strong>Total Amount:</strong></td>
                                <td style='padding: 10px; border: 1px solid #dee2e6;'>৳{totalAmount:N2} BDT</td>
                            </tr>
                            <tr style='background: #f8f9fa;'>
                                <td style='padding: 10px; border: 1px solid #dee2e6;'><strong>Payment Method:</strong></td>
                                <td style='padding: 10px; border: 1px solid #dee2e6;'>{paymentMethod}</td>
                            </tr>
                        </table>

                        <p>You can track the status of your order anytime on our website at <a href='{trackUrl}' style='color: #e53637;'>Track Your Order</a>.</p>
                        <p style='color: #777; font-size: 13px; margin-top: 30px;'>MaleFashion Team<br />Support Hotline: +880 1234-567890</p>
                    </div>";

                await _emailService.SendEmailAsync(customerName, customerEmail, subject, body);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send order confirmation email for {OrderNumber} to {Email}", orderNumber, customerEmail);
            }
        }
    }
}

