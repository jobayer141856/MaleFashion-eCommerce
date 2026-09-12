using Cortex.Mediator.Commands;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Contracts.Services;
using MaleFashion.Application.Exceptions;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.Features.CMS.Testimonials.Commands
{
    public class TestimonialDeleteCommandHandler : ICommandHandler<TestimonialDeleteCommand>
    {
        private readonly IApplicationUnitOfWork _unitOfWork;
        private readonly IFileStorageService _fileStorageService;
        private readonly ILogger<TestimonialDeleteCommandHandler> _logger;

        public TestimonialDeleteCommandHandler(
            IApplicationUnitOfWork unitOfWork,
            IFileStorageService fileStorageService,
            ILogger<TestimonialDeleteCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _fileStorageService = fileStorageService;
            _logger = logger;
        }

        public async Task Handle(TestimonialDeleteCommand command, CancellationToken cancellationToken)
        {
            var testimonial = await _unitOfWork.TestimonialRepository.GetByIdAsync(command.Id, cancellationToken);
            if (testimonial == null)
                throw new NotFoundException($"Testimonial with ID '{command.Id}' not found.");

            var avatarUrl = testimonial.AvatarUrl;
            await _unitOfWork.TestimonialRepository.RemoveAsync(command.Id, cancellationToken);
            await _unitOfWork.SaveAsync(cancellationToken);

            if (!string.IsNullOrEmpty(avatarUrl))
            {
                try
                {
                    await _fileStorageService.DeleteImageAsync(avatarUrl, "testimonials", cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to delete testimonial avatar '{AvatarUrl}' for ID '{Id}'", avatarUrl, command.Id);
                }
            }
        }
    }
}

