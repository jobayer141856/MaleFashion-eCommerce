using Cortex.Mediator;
using MaleFashion.Application.Contracts.Services;
using MaleFashion.Application.Features.Wishlists.Commands;
using MaleFashion.Application.Features.Wishlists.DTOs;
using MaleFashion.Application.Features.Wishlists.Queries;
using MaleFashion.Web.Models.Wishlist;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Web.Controllers
{
    public class WishlistController : Controller
    {
        private readonly IMediator _mediator;
        private readonly IWishlistSessionService _wishlistSessionService;

        public WishlistController(
            IMediator mediator,
            IWishlistSessionService wishlistSessionService)
        {
            _mediator = mediator;
            _wishlistSessionService = wishlistSessionService;
        }

        // GET: /Wishlist
        [HttpGet]
        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            var (customerId, guestProductIds) = await _wishlistSessionService.GetWishlistIdentityAsync(cancellationToken);

            var wishlist = await _mediator.SendQueryAsync<GetCustomerWishlistQuery, CustomerWishlistDto>(
                new GetCustomerWishlistQuery
                {
                    CustomerId = customerId,
                    GuestProductIds = guestProductIds
                }, cancellationToken);

            var model = new WishlistIndexModel
            {
                Wishlist = wishlist ?? new CustomerWishlistDto(),
                Currency = "BDT"
            };

            return View(model);
        }

        // POST: /Wishlist/Toggle
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Toggle(Guid productId, Guid? variantId, CancellationToken cancellationToken)
        {
            if (productId == Guid.Empty)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "Invalid product." });
                }
                return RedirectToAction(nameof(Index));
            }

            var (customerId, _) = await _wishlistSessionService.GetWishlistIdentityAsync(cancellationToken);

            bool isSuccess = true;
            bool isAdded;
            int totalCount;
            string message;

            if (customerId.HasValue)
            {
                var result = await _mediator.SendCommandAsync<ToggleWishlistCommand, ToggleWishlistResultDto>(
                    new ToggleWishlistCommand
                    {
                        CustomerId = customerId,
                        ProductId = productId,
                        VariantId = variantId
                    }, cancellationToken);

                isSuccess = result.Success;
                isAdded = result.IsAdded;
                totalCount = result.TotalCount;
                message = result.Message;
            }
            else
            {
                // Guest cookie toggle
                isAdded = _wishlistSessionService.ToggleGuestWishlist(productId);
                var guestList = _wishlistSessionService.GetGuestWishlist();
                totalCount = guestList.Count;
                message = isAdded ? "Product added to wishlist!" : "Product removed from wishlist.";
            }

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new
                {
                    success = isSuccess,
                    isAdded = isAdded,
                    message = message,
                    totalCount = totalCount
                });
            }

            TempData[isSuccess ? "SuccessMessage" : "ErrorMessage"] = message;
            return RedirectToAction(nameof(Index));
        }

        // POST: /Wishlist/Remove
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(Guid productId, CancellationToken cancellationToken)
        {
            if (productId == Guid.Empty)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "Invalid product." });
                }
                return RedirectToAction(nameof(Index));
            }

            var (customerId, _) = await _wishlistSessionService.GetWishlistIdentityAsync(cancellationToken);
            int totalCount;

            if (customerId.HasValue)
            {
                await _mediator.SendCommandAsync<RemoveWishlistItemCommand, bool>(
                    new RemoveWishlistItemCommand
                    {
                        CustomerId = customerId,
                        ProductId = productId
                    }, cancellationToken);

                var wishlist = await _mediator.SendQueryAsync<GetCustomerWishlistQuery, CustomerWishlistDto>(
                    new GetCustomerWishlistQuery
                    {
                        CustomerId = customerId
                    }, cancellationToken);

                totalCount = wishlist?.TotalCount ?? 0;
            }
            else
            {
                _wishlistSessionService.RemoveFromGuestWishlist(productId);
                totalCount = _wishlistSessionService.GetGuestWishlist().Count;
            }

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new
                {
                    success = true,
                    message = "Item removed from wishlist.",
                    totalCount = totalCount
                });
            }

            TempData["SuccessMessage"] = "Item removed from wishlist.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Wishlist/GetCount
        [HttpGet]
        public async Task<IActionResult> GetCount(CancellationToken cancellationToken)
        {
            var (customerId, guestProductIds) = await _wishlistSessionService.GetWishlistIdentityAsync(cancellationToken);

            var wishlist = await _mediator.SendQueryAsync<GetCustomerWishlistQuery, CustomerWishlistDto>(
                new GetCustomerWishlistQuery
                {
                    CustomerId = customerId,
                    GuestProductIds = guestProductIds
                }, cancellationToken);

            return Json(new { count = wishlist?.TotalCount ?? 0 });
        }
    }
}

