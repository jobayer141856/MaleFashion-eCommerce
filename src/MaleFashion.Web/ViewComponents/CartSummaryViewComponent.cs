using Cortex.Mediator;
using MaleFashion.Application.Contracts.Services;
using MaleFashion.Application.Features.Carts.DTOs;
using MaleFashion.Application.Features.Carts.Queries;
using MaleFashion.Web.Models.Cart;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace MaleFashion.Web.ViewComponents
{
    public class CartSummaryViewComponent : ViewComponent
    {
        private readonly IMediator _mediator;
        private readonly ICartSessionService _cartSessionService;

        public CartSummaryViewComponent(
            IMediator mediator,
            ICartSessionService cartSessionService)
        {
            _mediator = mediator;
            _cartSessionService = cartSessionService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var (customerId, sessionId) = await _cartSessionService.GetCartIdentityAsync();
            var cart = await _mediator.SendQueryAsync<GetCustomerCartQuery, CustomerCartDto>(
                new GetCustomerCartQuery
                {
                    CustomerId = customerId,
                    SessionId = sessionId
                });

            var model = new CartSummaryModel
            {
                TotalItemCount = cart?.TotalItemCount ?? 0,
                TotalAmount = cart?.Total ?? 0m,
                Currency = "BDT"
            };

            return View(model);
        }
    }
}

