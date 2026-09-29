using Cortex.Mediator.Commands;
using MaleFashion.Application.Contracts;
using MaleFashion.Domain.Entities.Carts;
using MaleFashion.Domain.Entities.Catalogs;
using MaleFashion.Domain.ValueOfObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.Features.Carts.Commands
{
    public class AddToCartCommandHandler : ICommandHandler<AddToCartCommand, bool>
    {
        private readonly IApplicationUnitOfWork _unitOfWork;

        public AddToCartCommandHandler(IApplicationUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(AddToCartCommand command, CancellationToken cancellationToken)
        {
            if (command.ProductId == Guid.Empty || command.Quantity <= 0)
                return false;

            var product = await _unitOfWork.ProductRepository.GetProductDetailsById(command.ProductId, cancellationToken);
            if (product == null || !product.IsActive)
                return false;

            // Resolve variant if specified
            ProductVariant? variant = null;
            if (command.ProductVariantId.HasValue && product.Variants != null)
            {
                variant = product.Variants.FirstOrDefault(v => v.Id == command.ProductVariantId.Value);
            }
            else if (product.Variants != null && (!string.IsNullOrWhiteSpace(command.Size) || !string.IsNullOrWhiteSpace(command.Color)))
            {
                variant = product.Variants.FirstOrDefault(v =>
                    (string.IsNullOrWhiteSpace(command.Size) || string.Equals(v.Size, command.Size, StringComparison.OrdinalIgnoreCase)) &&
                    (string.IsNullOrWhiteSpace(command.Color) || string.Equals(v.ColorName, command.Color, StringComparison.OrdinalIgnoreCase) || string.Equals(v.ColorHex, command.Color, StringComparison.OrdinalIgnoreCase)));
            }

            // Fallback to first variant if product has variants
            if (variant == null && product.Variants != null && product.Variants.Any())
            {
                variant = product.Variants.FirstOrDefault(v => v.StockQuantity > 0) ?? product.Variants.FirstOrDefault();
            }

            // Query active deals for product
            var activeDeals = await _unitOfWork.ProductRepository.GetActiveDealsForProductIdsAsync(
                new List<Guid> { product.Id }, cancellationToken);
            var activeDeal = (activeDeals != null && activeDeals.TryGetValue(product.Id, out var deal)) ? deal : null;

            decimal basePriceAmount = product.BasePrice?.Amount ?? 0m;
            decimal priceAmount;

            if (activeDeal != null && activeDeal.SalePrice > 0 && activeDeal.SalePrice < basePriceAmount)
            {
                priceAmount = activeDeal.SalePrice;
            }
            else if (variant?.VariantPrice != null && variant.VariantPrice.Amount > 0 && variant.VariantPrice.Amount != basePriceAmount)
            {
                priceAmount = variant.VariantPrice.Amount;
            }
            else if (product.SalePrice != null && product.SalePrice.Amount > 0 && product.SalePrice.Amount < basePriceAmount)
            {
                priceAmount = product.SalePrice.Amount;
            }
            else
            {
                priceAmount = basePriceAmount;
            }

            string currency = product.BasePrice?.Currency ?? "BDT";
            var unitPrice = new Money(priceAmount, currency);

            string rawImg = product.Images?.FirstOrDefault(i => i.IsPrimary)?.ImageUrl
                ?? product.Images?.FirstOrDefault()?.ImageUrl
                ?? string.Empty;

            string imageUrl = !string.IsNullOrWhiteSpace(rawImg)
                ? (rawImg.StartsWith("/") || rawImg.StartsWith("http") ? rawImg : $"/uploads/products/{rawImg}")
                : "/malefashion/img/product/product-1.jpg";

            // Find or create Cart
            var cart = await _unitOfWork.CartRepository.GetActiveCartByCustomerOrSessionAsync(
                command.CustomerId, command.SessionId, cancellationToken);

            if (cart == null)
            {
                cart = new Cart
                {
                    Id = Guid.NewGuid(),
                    CustomerId = command.CustomerId,
                    SessionId = !string.IsNullOrWhiteSpace(command.SessionId) ? command.SessionId : Guid.NewGuid().ToString(),
                    CreatedAt = DateTime.UtcNow
                };
                await _unitOfWork.CartRepository.AddAsync(cart, cancellationToken);
            }
            else if (!cart.CustomerId.HasValue && command.CustomerId.HasValue)
            {
                cart.CustomerId = command.CustomerId.Value;
            }

            // Check if item already exists in cart
            var existingItem = cart.Items.FirstOrDefault(i =>
                i.ProductId == product.Id &&
                ((variant == null && !i.ProductVariantId.HasValue) || (variant != null && i.ProductVariantId == variant.Id)));

            if (existingItem != null)
            {
                existingItem.Quantity += command.Quantity;
                existingItem.UnitPrice = unitPrice;
                existingItem.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                var newItem = new CartItem
                {
                    CartId = cart.Id,
                    ProductId = product.Id,
                    ProductVariantId = variant?.Id,
                    ProductName = product.Name,
                    Quantity = command.Quantity,
                    UnitPrice = unitPrice,
                    ImageUrl = imageUrl,
                    CreatedAt = DateTime.UtcNow
                };
                cart.Items.Add(newItem);
            }

            cart.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.SaveAsync(cancellationToken);
            return true;
        }
    }
}

