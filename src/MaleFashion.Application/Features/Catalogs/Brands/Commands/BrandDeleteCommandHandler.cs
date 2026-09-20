using Cortex.Mediator.Commands;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Contracts.Services;
using MaleFashion.Application.Exceptions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace MaleFashion.Application.Features.Catalogs.Brands.Commands
{
    public class BrandDeleteCommandHandler : ICommandHandler<BrandDeleteCommand>
    {
        private readonly IApplicationUnitOfWork _unitOfWork;
        private readonly IFileStorageService _fileStorageService;
        private readonly ILogger<BrandDeleteCommandHandler> _logger;
        public BrandDeleteCommandHandler(
            IApplicationUnitOfWork unitOfWork,
            IFileStorageService fileStorageService,
            ILogger<BrandDeleteCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _fileStorageService = fileStorageService;
            _logger = logger;
        }
        public async Task Handle(BrandDeleteCommand command, CancellationToken cancellationToken)
        {
            var brand = await _unitOfWork.BrandRepository.GetByIdAsync(command.Id, cancellationToken);
            if (brand == null)
                throw new NotFoundException($"Brand with ID '{command.Id}' not found.");

            var productCount = await _unitOfWork.ProductRepository.GetCountAsync(
                p => p.BrandId == command.Id, cancellationToken);

            if (productCount > 0)
                throw new DuplicateDataException($"Brand '{brand.Name}' cannot be deleted because it is associated with {productCount} product(s). Please reassign or delete the products first.");

            var logoUrl = brand.LogoUrl;
            await _unitOfWork.BrandRepository.RemoveAsync(command.Id, cancellationToken);
            await _unitOfWork.SaveAsync(cancellationToken);
            if (!string.IsNullOrEmpty(logoUrl))
            {
                try
                {
                    await _fileStorageService.DeleteImageAsync(logoUrl, "brands", cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to delete brand logo '{LogoUrl}' for brand ID '{Id}'", logoUrl, command.Id);
                }
            }
        }
    }
}
