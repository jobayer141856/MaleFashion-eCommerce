using Cortex.Mediator.Commands;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Exceptions;
using MaleFashion.Domain.Entities.Catalogs;
using MaleFashion.Domain.Utilities;
using MaleFashion.Domain.ValueOfObjects;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.Features.Catalogs.Products.Commands
{
    public class ProductUpdateCommandHandler : ICommandHandler<ProductUpdateCommand, Product>
    {
        private readonly IApplicationUnitOfWork _unitOfWork;

        public ProductUpdateCommandHandler(IApplicationUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Product> Handle(ProductUpdateCommand command, CancellationToken cancellationToken)
        {
            var isDuplicateSku = await _unitOfWork.ProductRepository.IsDuplicateSku(command.Sku, command.Id, cancellationToken);
            if (isDuplicateSku)
            {
                throw new DuplicateDataException($"Product SKU '{command.Sku}' already exists.");
            }

            var product = await _unitOfWork.ProductRepository.GetProductDetailsById(command.Id, cancellationToken);
            if (product == null)
            {
                throw new NotFoundException($"Product with ID '{command.Id}' not found.");
            }

            product.Name = command.Name;
            product.Sku = command.Sku;
            product.Description = command.Description;
            product.AdditionalInformation = command.AdditionalInformation;
            product.BasePrice = new Money(command.BasePrice);
            product.SalePrice = command.SalePrice.HasValue ? new Money(command.SalePrice.Value) : null;
            product.CategoryId = command.CategoryId;
            product.BrandId = command.BrandId;
            product.IsActive = command.IsActive;
            product.IsNewArrival = command.IsNewArrival;
            product.IsHotSale = command.IsHotSale;
            product.UpdatedAt = DateTime.UtcNow;

            // Update Primary Image
            if (!string.IsNullOrEmpty(command.PrimaryImageName))
            {
                var existingPrimary = product.Images.FirstOrDefault(i => i.IsPrimary);
                if (existingPrimary != null)
                {
                    existingPrimary.ImageUrl = command.PrimaryImageName;
                    existingPrimary.UpdatedAt = DateTime.UtcNow;
                }
                else
                {
                    product.Images.Add(new ProductImage
                    {
                        Id = IdentityGenerator.NewSequentialGuid(),
                        ProductId = product.Id,
                        ImageUrl = command.PrimaryImageName,
                        IsPrimary = true,
                        DisplayOrder = 1,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }

            // Remove Deleted Gallery Images
            if (command.DeletedImageIds != null && command.DeletedImageIds.Count > 0)
            {
                var imagesToRemove = product.Images.Where(i => command.DeletedImageIds.Contains(i.Id)).ToList();
                foreach (var img in imagesToRemove)
                {
                    product.Images.Remove(img);
                }
            }

            // Add New Gallery Images
            if (command.NewGalleryImageNames != null && command.NewGalleryImageNames.Count > 0)
            {
                int order = product.Images.Count + 1;
                foreach (var imgName in command.NewGalleryImageNames)
                {
                    product.Images.Add(new ProductImage
                    {
                        Id = IdentityGenerator.NewSequentialGuid(),
                        ProductId = product.Id,
                        ImageUrl = imgName,
                        IsPrimary = false,
                        DisplayOrder = order++,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }

            // Update Variants
            // Update Variants (Synchronize in-place to avoid deleting active variants)
            if (command.Variants != null)
            {
                var incomingIds = command.Variants
                    .Where(v => v.Id.HasValue && v.Id.Value != Guid.Empty)
                    .Select(v => v.Id!.Value)
                    .ToHashSet();

                // 1. Remove variants that are no longer in the submitted list
                var variantsToRemove = product.Variants
                    .Where(v => !incomingIds.Contains(v.Id))
                    .ToList();

                foreach (var removedVariant in variantsToRemove)
                {
                    product.Variants.Remove(removedVariant);
                }

                // 2. Update existing variants or add new ones
                foreach (var v in command.Variants)
                {
                    var existingVariant = v.Id.HasValue && v.Id.Value != Guid.Empty
                        ? product.Variants.FirstOrDefault(x => x.Id == v.Id.Value)
                        : null;

                    if (existingVariant != null)
                    {
                        existingVariant.Size = string.IsNullOrWhiteSpace(v.Size) ? "Standard" : v.Size;
                        existingVariant.ColorName = v.ColorName ?? string.Empty;
                        existingVariant.ColorHex = string.IsNullOrWhiteSpace(v.ColorHex) ? "#000000" : v.ColorHex;
                        existingVariant.Sku = string.IsNullOrWhiteSpace(v.Sku) ? $"{product.Sku}-{v.Size}" : v.Sku;
                        existingVariant.StockQuantity = v.StockQuantity;
                        existingVariant.lowStockQuantity = v.LowStockQuantity;
                        existingVariant.VariantPrice = v.Price.HasValue ? new Money(v.Price.Value) : null;
                        existingVariant.UpdatedAt = DateTime.UtcNow;
                    }
                    else
                    {
                        product.Variants.Add(new ProductVariant
                        {
                            Id = IdentityGenerator.NewSequentialGuid(),
                            ProductId = product.Id,
                            Size = string.IsNullOrWhiteSpace(v.Size) ? "Standard" : v.Size,
                            ColorName = v.ColorName ?? string.Empty,
                            ColorHex = string.IsNullOrWhiteSpace(v.ColorHex) ? "#000000" : v.ColorHex,
                            Sku = string.IsNullOrWhiteSpace(v.Sku) ? $"{product.Sku}-{v.Size}" : v.Sku,
                            StockQuantity = v.StockQuantity,
                            lowStockQuantity = v.LowStockQuantity,
                            VariantPrice = v.Price.HasValue ? new Money(v.Price.Value) : null,
                            CreatedAt = DateTime.UtcNow
                        });
                    }
                }
            }

            await _unitOfWork.ProductRepository.EditAsync(product, cancellationToken);
            await _unitOfWork.SaveAsync(cancellationToken);

            return product;
        }
    }
}

