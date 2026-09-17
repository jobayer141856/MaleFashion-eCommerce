using Cortex.Mediator;
using MaleFashion.Application.Contracts.Services;
using MaleFashion.Application.Features.Wishlists.DTOs;
using MaleFashion.Application.Features.Wishlists.Queries;
using MaleFashion.Web.Models.Wishlist;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace MaleFashion.Web.ViewComponents
{
    public class WishlistSummaryViewComponent : ViewComponent
    {
        private readonly IMediator _mediator;
        private readonly IWishlistSessionService _wishlistSessionService;

        public WishlistSummaryViewComponent(
            IMediator mediator,
            IWishlistSessionService wishlistSessionService)
        {
            _mediator = mediator;
            _wishlistSessionService = wishlistSessionService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var (customerId, guestProductIds) = await _wishlistSessionService.GetWishlistIdentityAsync();

            var wishlist = await _mediator.SendQueryAsync<GetCustomerWishlistQuery, CustomerWishlistDto>(
                new GetCustomerWishlistQuery
                {
                    CustomerId = customerId,
                    GuestProductIds = guestProductIds
                });

            var model = new WishlistSummaryModel
            {
                TotalCount = wishlist?.TotalCount ?? 0
            };

            return View(model);
        }
    }
}

