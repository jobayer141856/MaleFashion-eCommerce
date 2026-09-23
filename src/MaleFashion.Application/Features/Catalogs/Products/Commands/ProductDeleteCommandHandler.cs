using Cortex.Mediator.Commands;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Contracts.Services;
using MaleFashion.Application.Exceptions;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.Features.Catalogs.Products.Commands
{
    public class ProductDeleteCommandHandler : ICommandHandler<ProductDeleteCommand>
    {
        private readonly IApplicationUnitOfWork _unitOfWork;
        private readonly IFileStorageService _fileStorageService;
        private readonly ILogger<ProductDeleteCommandHandler> _logger;

        public ProductDeleteCommandHandler(
            IApplicationUnitOfWork unitOfWork,
            IFileStorageService fileStorageService,
            ILogger<ProductDeleteCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _fileStorageService = fileStorageService;
            _logger = logger;
        }

        public async Task Handle(ProductDeleteCommand command, CancellationToken cancellationToken)
        {
            var product = await _unitOfWork.ProductRepository.GetProductDetailsById(command.Id, cancellationToken);
            if (product == null)
            {
                throw new NotFoundException($"Product with ID '{command.Id}' not found.");
            }

            // 1. Check if product is in any customer orders
            var orderCount = await _unitOfWork.OrderRepository.GetCountAsync(
                o => o.OrderItems.Any(oi => oi.ProductId == command.Id), cancellationToken);
            if (orderCount > 0)
            {
                throw new DuplicateDataException(
                    $"Product '{product.Name}' cannot be deleted because it is part of {orderCount} customer order(s). Consider deactivating it instead.");
            }
            // 2. Check if product is in any active customer carts
            var cartCount = await _unitOfWork.CartRepository.GetCountAsync(
                c => c.Items.Any(ci => ci.ProductId == command.Id), cancellationToken);
            if (cartCount > 0)
            {
                throw new DuplicateDataException(
                    $"Product '{product.Name}' cannot be deleted because it is currently in {cartCount} active shopping cart(s).");
            }

            var imageNames = product.Images.Select(i => i.ImageUrl).ToList();

            await _unitOfWork.ProductRepository.RemoveAsync(command.Id, cancellationToken);
            await _unitOfWork.SaveAsync(cancellationToken);

            // Clean up image files from disk
            foreach (var imgName in imageNames)
            {
                if (!string.IsNullOrEmpty(imgName))
                {
                    try
                    {
                        await _fileStorageService.DeleteImageAsync(imgName, "products", cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to delete product image '{Image}' for product ID '{Id}'", imgName, command.Id);
                    }
                }
            }
        }
    }
}

