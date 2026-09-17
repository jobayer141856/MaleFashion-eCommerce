using Cortex.Mediator.Queries;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Features.Carts.DTOs;
using MaleFashion.Domain.Enums;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.Features.Carts.Queries
{
    public class GetCustomerCartQueryHandler : IQueryHandler<GetCustomerCartQuery, CustomerCartDto>
    {
        private readonly IApplicationUnitOfWork _unitOfWork;

        public GetCustomerCartQueryHandler(IApplicationUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<CustomerCartDto> Handle(GetCustomerCartQuery query, CancellationToken cancellationToken)
        {
            if (!query.CustomerId.HasValue && string.IsNullOrWhiteSpace(query.SessionId))
                return new CustomerCartDto();

            var cart = await _unitOfWork.CartRepository.GetActiveCartByCustomerOrSessionAsync(
                query.CustomerId, query.SessionId, cancellationToken);

            if (cart == null || cart.Items == null || !cart.Items.Any())
                return new CustomerCartDto();

            var dto = new CustomerCartDto
            {
                CartId = cart.Id,
                AppliedCouponCode = cart.AppliedCouponCode,
                Currency = "BDT",
                Items = cart.Items.Select(item =>
                {
                    var img = !string.IsNullOrWhiteSpace(item.ImageUrl)
                        ? item.ImageUrl
                        : item.Product?.Images?.FirstOrDefault(i => i.IsPrimary)?.ImageUrl
                          ?? item.Product?.Images?.FirstOrDefault()?.ImageUrl
                          ?? "/malefashion/img/product/product-default.jpg";

                    return new CustomerCartItemDto
                    {
                        Id = item.Id,
                        ProductId = item.ProductId,
                        ProductVariantId = item.ProductVariantId,
                        ProductName = item.ProductName,
                        Size = item.ProductVariant?.Size,
                        ColorName = item.ProductVariant?.ColorName,
                        ImageUrl = img,
                        UnitPrice = item.UnitPrice != null ? item.UnitPrice.Amount : 0m,
                        Quantity = item.Quantity
                    };
                }).ToList()
            };

            // Calculate coupon discount if applicable
            if (!string.IsNullOrWhiteSpace(cart.AppliedCouponCode))
            {
                var now = DateTime.UtcNow;
                var coupons = await _unitOfWork.CouponRepository.GetAllAsync(cancellationToken);
                var coupon = coupons.FirstOrDefault(c =>
                    string.Equals(c.Code, cart.AppliedCouponCode, StringComparison.OrdinalIgnoreCase) &&
                    c.IsActive &&
                    (!c.StartDate.HasValue || c.StartDate <= now) &&
                    c.ExpirationDate >= now);

                if (coupon != null && (!coupon.MinimumSpend.HasValue || dto.Subtotal >= coupon.MinimumSpend.Value))
                {
                    if (coupon.DiscountType == DiscountType.Percentage)
                    {
                        dto.DiscountAmount = Math.Round((dto.Subtotal * coupon.DiscountValue) / 100m, 2);
                    }
                    else
                    {
                        dto.DiscountAmount = Math.Min(coupon.DiscountValue, dto.Subtotal);
                    }
                }
            }

            return dto;
        }
    }
}

