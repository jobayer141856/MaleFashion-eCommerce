using Cortex.Mediator.Queries;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Features.Wishlists.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.Features.Wishlists.Queries
{
    public class GetCustomerWishlistQueryHandler : IQueryHandler<GetCustomerWishlistQuery, CustomerWishlistDto>
    {
        private readonly IApplicationUnitOfWork _unitOfWork;

        public GetCustomerWishlistQueryHandler(IApplicationUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<CustomerWishlistDto> Handle(GetCustomerWishlistQuery query, CancellationToken cancellationToken)
        {
            var result = new CustomerWishlistDto();
            var itemDtos = new List<CustomerWishlistItemDto>();

            // 1. Authenticated Customer
            if (query.CustomerId.HasValue && query.CustomerId.Value != Guid.Empty)
            {
                var customer = await _unitOfWork.CustomerRepository.GetCustomerWithWishlistByIdAsync(query.CustomerId.Value, cancellationToken);
                if (customer != null && customer.Wishlist != null && customer.Wishlist.Any())
                {
                    foreach (var w in customer.Wishlist.OrderByDescending(x => x.CreatedAt))
                    {
                        if (w.Product != null && w.Product.IsActive)
                        {
                            var primaryImg = w.Product.Images?.OrderByDescending(i => i.IsPrimary).ThenBy(i => i.DisplayOrder).FirstOrDefault()?.ImageUrl
                                ?? w.Product.Images?.FirstOrDefault()?.ImageUrl;
                            var price = (w.Product.SalePrice ?? w.Product.BasePrice)?.Amount ?? 0m;
                            var totalStock = w.Product.Variants?.Sum(v => v.StockQuantity) ?? 0;

                            itemDtos.Add(new CustomerWishlistItemDto
                            {
                                Id = w.Id,
                                ProductId = w.ProductId,
                                ProductName = w.Product.Name,
                                Sku = w.Product.Sku,
                                ImageUrl = primaryImg,
                                UnitPrice = price,
                                Currency = "BDT",
                                IsInStock = totalStock > 0 || (w.Product.Variants != null && !w.Product.Variants.Any()),
                                StockQuantity = totalStock,
                                AddedAt = w.CreatedAt
                            });
                        }
                    }
                }
            }
            // 2. Guest items via Product IDs
            else if (query.GuestProductIds != null && query.GuestProductIds.Any())
            {
                var distinctIds = query.GuestProductIds.Distinct().ToList();
                var products = await _unitOfWork.ProductRepository.GetProductsByIdsAsync(distinctIds, cancellationToken);

                foreach (var p in products)
                {
                    var primaryImg = p.Images?.OrderByDescending(i => i.IsPrimary).ThenBy(i => i.DisplayOrder).FirstOrDefault()?.ImageUrl
                        ?? p.Images?.FirstOrDefault()?.ImageUrl;
                    var price = (p.SalePrice ?? p.BasePrice)?.Amount ?? 0m;
                    var totalStock = p.Variants?.Sum(v => v.StockQuantity) ?? 0;

                    itemDtos.Add(new CustomerWishlistItemDto
                    {
                        Id = p.Id,
                        ProductId = p.Id,
                        ProductName = p.Name,
                        Sku = p.Sku,
                        ImageUrl = primaryImg,
                        UnitPrice = price,
                        Currency = "BDT",
                        IsInStock = totalStock > 0 || (p.Variants != null && !p.Variants.Any()),
                        StockQuantity = totalStock,
                        AddedAt = DateTime.UtcNow
                    });
                }
            }

            result.Items = itemDtos;
            return result;
        }
    }
}
