using Cortex.Mediator.Commands;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Exceptions;
using MaleFashion.Domain.Entities.Catalogs;
using MaleFashion.Domain.Utilities;
using MaleFashion.Domain.ValueOfObjects;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.Features.Catalogs.Products.Commands
{
    public class ProductAddCommandHandler : ICommandHandler<ProductAddCommand, Product>
    {
        private readonly IApplicationUnitOfWork _unitOfWork;

        public ProductAddCommandHandler(IApplicationUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Product> Handle(ProductAddCommand command, CancellationToken cancellationToken)
        {
            var isDuplicateSku = await _unitOfWork.ProductRepository.IsDuplicateSku(command.Sku, null, cancellationToken);
            if (isDuplicateSku)
            {
                throw new DuplicateDataException($"Product SKU '{command.Sku}' already exists.");
            }

            var productId = IdentityGenerator.NewSequentialGuid();
            var product = new Product
            {
                Id = productId,
                Name = command.Name,
                Sku = command.Sku,
                Description = command.Description,
                AdditionalInformation = command.AdditionalInformation,
                BasePrice = new Money(command.BasePrice),
                SalePrice = command.SalePrice.HasValue ? new Money(command.SalePrice.Value) : null,
                CategoryId = command.CategoryId,
                BrandId = command.BrandId,
                IsActive = command.IsActive,
                IsNewArrival = command.IsNewArrival,
                IsHotSale = command.IsHotSale,
                Rating = 0,
                CreatedAt = DateTime.UtcNow
            };

            // Primary Image
            if (!string.IsNullOrEmpty(command.PrimaryImageName))
            {
                product.Images.Add(new ProductImage
                {
                    Id = IdentityGenerator.NewSequentialGuid(),
                    ProductId = productId,
                    ImageUrl = command.PrimaryImageName,
                    IsPrimary = true,
                    DisplayOrder = 1,
                    CreatedAt = DateTime.UtcNow
                });
            }

            // Gallery Images
            if (command.GalleryImageNames != null && command.GalleryImageNames.Count > 0)
            {
                int order = 2;
                foreach (var imgName in command.GalleryImageNames)
                {
                    product.Images.Add(new ProductImage
                    {
                        Id = IdentityGenerator.NewSequentialGuid(),
                        ProductId = productId,
                        ImageUrl = imgName,
                        IsPrimary = false,
                        DisplayOrder = order++,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }

            // Variants
            if (command.Variants != null && command.Variants.Count > 0)
            {
                foreach (var v in command.Variants)
                {
                    var colorSlug = !string.IsNullOrWhiteSpace(v.ColorName) ? $"-{v.ColorName.Trim().Replace(" ", "")}" : "";
                    decimal variantPriceAmount = (v.Price.HasValue && v.Price.Value > 0)
                        ? v.Price.Value
                        : product.BasePrice.Amount;

                    product.Variants.Add(new ProductVariant
                    {
                        Id = IdentityGenerator.NewSequentialGuid(),
                        ProductId = productId,
                        Size = string.IsNullOrWhiteSpace(v.Size) ? "Standard" : v.Size,
                        ColorName = v.ColorName ?? string.Empty,
                        ColorHex = string.IsNullOrWhiteSpace(v.ColorHex) ? "#000000" : v.ColorHex,
                        Sku = string.IsNullOrWhiteSpace(v.Sku) ? $"{product.Sku}-{v.Size}{colorSlug}" : v.Sku,
                        StockQuantity = v.StockQuantity,
                        lowStockQuantity = v.LowStockQuantity,
                        VariantPrice = new Money(variantPriceAmount),
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }

            // Tags
            if (command.Tags != null && command.Tags.Count > 0)
            {
                foreach (var tag in command.Tags)
                {
                    if (!string.IsNullOrWhiteSpace(tag))
                    {
                        product.Tags.Add(new ProductTag
                        {
                            Id = IdentityGenerator.NewSequentialGuid(),
                            ProductId = productId,
                            Name = tag.Trim(),
                            CreatedAt = DateTime.UtcNow
                        });
                    }
                }
            }

            await _unitOfWork.ProductRepository.AddAsync(product, cancellationToken);
            await _unitOfWork.SaveAsync(cancellationToken);

            return product;
        }
    }
}

