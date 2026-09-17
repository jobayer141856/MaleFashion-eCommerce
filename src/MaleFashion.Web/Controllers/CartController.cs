using Cortex.Mediator;
using MaleFashion.Application.Contracts.Services;
using MaleFashion.Application.Features.Carts.Commands;
using MaleFashion.Application.Features.Carts.DTOs;
using MaleFashion.Application.Features.Carts.Queries;
using MaleFashion.Web.Models.Cart;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Web.Controllers
{
    public class CartController : Controller
    {
        private readonly IMediator _mediator;
        private readonly ICartSessionService _cartSessionService;

        public CartController(
            IMediator mediator,
            ICartSessionService cartSessionService)
        {
            _mediator = mediator;
            _cartSessionService = cartSessionService;
        }

        // GET: /Cart
        [HttpGet]
        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            var (customerId, sessionId) = await _cartSessionService.GetCartIdentityAsync(cancellationToken);
            var cart = await _mediator.SendQueryAsync<GetCustomerCartQuery, CustomerCartDto>(
                new GetCustomerCartQuery
                {
                    CustomerId = customerId,
                    SessionId = sessionId
                }, cancellationToken);

            var model = new ShoppingCartModel
            {
                Cart = cart ?? new CustomerCartDto(),
                CouponErrorMessage = TempData["CouponError"]?.ToString(),
                CouponSuccessMessage = TempData["CouponSuccess"]?.ToString()
            };

            return View(model);
        }

        // POST: /Cart/AddToCart
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart(
            Guid productId,
            Guid? variantId,
            string? size,
            string? color,
            int quantity = 1,
            CancellationToken cancellationToken = default)
        {
            var (customerId, sessionId) = await _cartSessionService.GetCartIdentityAsync(cancellationToken);

            var success = await _mediator.SendCommandAsync<AddToCartCommand, bool>(
                new AddToCartCommand
                {
                    CustomerId = customerId,
                    SessionId = sessionId,
                    ProductId = productId,
                    ProductVariantId = variantId,
                    Size = size,
                    Color = color,
                    Quantity = quantity > 0 ? quantity : 1
                }, cancellationToken);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                var updatedCart = await _mediator.SendQueryAsync<GetCustomerCartQuery, CustomerCartDto>(
                    new GetCustomerCartQuery
                    {
                        CustomerId = customerId,
                        SessionId = sessionId
                    }, cancellationToken);

                return Json(new
                {
                    success,
                    message = success ? "Product added to cart!" : "Unable to add product to cart.",
                    totalItemCount = updatedCart?.TotalItemCount ?? 0,
                    totalAmount = updatedCart?.Total ?? 0m,
                    currency = "BDT"
                });
            }

            if (success)
            {
                TempData["CartSuccess"] = "Product added to cart!";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: /Cart/UpdateCart
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateCart(Dictionary<Guid, int> quantities, CancellationToken cancellationToken)
        {
            var (customerId, sessionId) = await _cartSessionService.GetCartIdentityAsync(cancellationToken);

            await _mediator.SendCommandAsync<UpdateCartItemQuantityCommand, bool>(
                new UpdateCartItemQuantityCommand
                {
                    CustomerId = customerId,
                    SessionId = sessionId,
                    ItemQuantities = quantities ?? new Dictionary<Guid, int>()
                }, cancellationToken);

            return RedirectToAction(nameof(Index));
        }

        // POST: /Cart/RemoveItem/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveItem(Guid id, CancellationToken cancellationToken)
        {
            var (customerId, sessionId) = await _cartSessionService.GetCartIdentityAsync(cancellationToken);

            var success = await _mediator.SendCommandAsync<RemoveCartItemCommand, bool>(
                new RemoveCartItemCommand
                {
                    CustomerId = customerId,
                    SessionId = sessionId,
                    CartItemId = id
                }, cancellationToken);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                var updatedCart = await _mediator.SendQueryAsync<GetCustomerCartQuery, CustomerCartDto>(
                    new GetCustomerCartQuery
                    {
                        CustomerId = customerId,
                        SessionId = sessionId
                    }, cancellationToken);

                return Json(new
                {
                    success,
                    totalItemCount = updatedCart?.TotalItemCount ?? 0,
                    totalAmount = updatedCart?.Total ?? 0m,
                    subtotal = updatedCart?.Subtotal ?? 0m,
                    discount = updatedCart?.DiscountAmount ?? 0m,
                    currency = "BDT"
                });
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: /Cart/ApplyCoupon
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApplyCoupon(string couponCode, CancellationToken cancellationToken)
        {
            var (customerId, sessionId) = await _cartSessionService.GetCartIdentityAsync(cancellationToken);

            var (success, message) = await _mediator.SendCommandAsync<ApplyCartCouponCommand, (bool Success, string Message)>(
                new ApplyCartCouponCommand
                {
                    CustomerId = customerId,
                    SessionId = sessionId,
                    CouponCode = couponCode
                }, cancellationToken);

            if (success)
            {
                TempData["CouponSuccess"] = message;
            }
            else
            {
                TempData["CouponError"] = message;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}

