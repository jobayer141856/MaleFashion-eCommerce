using Cortex.Mediator.Commands;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Contracts.Services;
using MaleFashion.Application.Exceptions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace MaleFashion.Application.Features.Catalogs.Categories.Commands
{
    public class CategoryDeleteCommandHandler : ICommandHandler<CategoryDeleteCommand>
    {
        private readonly IApplicationUnitOfWork _unitOfWork;
        private readonly IFileStorageService _fileStorageService;
        private readonly ILogger<CategoryDeleteCommandHandler> _logger;

        public CategoryDeleteCommandHandler(IApplicationUnitOfWork unitOfWork, IFileStorageService fileStorageService, ILogger<CategoryDeleteCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _fileStorageService = fileStorageService;
            _logger = logger;
        }

        public async Task Handle(CategoryDeleteCommand command, CancellationToken cancellationToken)
        {
            var category = await _unitOfWork.CategoryRepository.GetByIdAsync(command.Id, cancellationToken);

            if (category == null)
                throw new NotFoundException($"Category with ID '{command.Id}' not found.");

            var productCount = await _unitOfWork.ProductRepository.GetCountAsync(
                p => p.CategoryId == command.Id, cancellationToken);

            if (productCount > 0)
                throw new DuplicateDataException($"Category '{category.Name}' cannot be deleted because it is associated with {productCount} product(s). Please reassign or delete the products first.");

            var imageName = category.ImageName;

            await _unitOfWork.CategoryRepository.RemoveAsync(command.Id, cancellationToken);
            await _unitOfWork.SaveAsync(cancellationToken);

            if (!string.IsNullOrEmpty(imageName))
            {
                try
                {
                    await _fileStorageService.DeleteImageAsync(imageName, "images", cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed to delete image '{imageName}' for category '{category.Name}'.");
                }
            }
        }
    }
}
