using Cortex.Mediator.Commands;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Contracts.Services;
using MaleFashion.Application.Exceptions;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.Features.CMS.HeroSlides.Commands
{
    public class HeroSlideDeleteCommandHandler : ICommandHandler<HeroSlideDeleteCommand>
    {
        private readonly IApplicationUnitOfWork _unitOfWork;
        private readonly IFileStorageService _fileStorageService;
        private readonly ILogger<HeroSlideDeleteCommandHandler> _logger;

        public HeroSlideDeleteCommandHandler(
            IApplicationUnitOfWork unitOfWork,
            IFileStorageService fileStorageService,
            ILogger<HeroSlideDeleteCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _fileStorageService = fileStorageService;
            _logger = logger;
        }

        public async Task Handle(HeroSlideDeleteCommand command, CancellationToken cancellationToken)
        {
            var slide = await _unitOfWork.HeroSlideRepository.GetByIdAsync(command.Id, cancellationToken);
            if (slide == null)
                throw new NotFoundException($"Hero slide with ID '{command.Id}' not found.");

            var img = slide.ImageUrl;
            await _unitOfWork.HeroSlideRepository.RemoveAsync(command.Id, cancellationToken);
            await _unitOfWork.SaveAsync(cancellationToken);

            if (!string.IsNullOrEmpty(img) && !img.StartsWith("http") && !img.StartsWith("~"))
            {
                try
                {
                    await _fileStorageService.DeleteImageAsync(img, "hero", cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to delete hero slide photo '{ImageUrl}' for ID '{Id}'", img, command.Id);
                }
            }
        }
    }
}

