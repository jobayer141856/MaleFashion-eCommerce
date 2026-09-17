using Cortex.Mediator.Commands;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Features.Wishlists.DTOs;
using MaleFashion.Domain.Entities.Customers;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.Features.Wishlists.Commands
{
    public class ToggleWishlistCommandHandler : ICommandHandler<ToggleWishlistCommand, ToggleWishlistResultDto>
    {
        private readonly IApplicationUnitOfWork _unitOfWork;

        public ToggleWishlistCommandHandler(IApplicationUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ToggleWishlistResultDto> Handle(ToggleWishlistCommand command, CancellationToken cancellationToken)
        {
            if (command.ProductId == Guid.Empty)
            {
                return new ToggleWishlistResultDto
                {
                    Success = false,
                    Message = "Invalid product identifier."
                };
            }

            var product = await _unitOfWork.ProductRepository.GetProductDetailsById(command.ProductId, cancellationToken);
            if (product == null || !product.IsActive)
            {
                return new ToggleWishlistResultDto
                {
                    Success = false,
                    Message = "Product not found or is currently inactive."
                };
            }

            // 1. Authenticated customer
            if (command.CustomerId.HasValue && command.CustomerId.Value != Guid.Empty)
            {
                var customer = await _unitOfWork.CustomerRepository.GetCustomerWithWishlistByIdAsync(command.CustomerId.Value, cancellationToken);
                if (customer == null)
                {
                    return new ToggleWishlistResultDto
                    {
                        Success = false,
                        Message = "Customer profile not found."
                    };
                }

                var existingItem = customer.Wishlist.FirstOrDefault(w => w.ProductId == command.ProductId);
                if (existingItem != null)
                {
                    customer.Wishlist.Remove(existingItem);
                    await _unitOfWork.SaveAsync(cancellationToken);

                    return new ToggleWishlistResultDto
                    {
                        Success = true,
                        IsAdded = false,
                        Message = $"{product.Name} removed from wishlist.",
                        TotalCount = customer.Wishlist.Count
                    };
                }
                else
                {
                    var newItem = new WishlistItem
                    {
                        Id = Guid.NewGuid(),
                        CustomerId = customer.Id,
                        ProductId = product.Id,
                        ProductVariantId = command.VariantId,
                        CreatedAt = DateTime.UtcNow
                    };
                    customer.Wishlist.Add(newItem);
                    await _unitOfWork.SaveAsync(cancellationToken);

                    return new ToggleWishlistResultDto
                    {
                        Success = true,
                        IsAdded = true,
                        Message = $"{product.Name} added to wishlist!",
                        TotalCount = customer.Wishlist.Count
                    };
                }
            }

            // 2. Guest fallback
            return new ToggleWishlistResultDto
            {
                Success = true,
                IsAdded = true,
                Message = $"{product.Name} added to wishlist!"
            };
        }
    }
}
