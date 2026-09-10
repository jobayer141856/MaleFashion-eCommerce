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

