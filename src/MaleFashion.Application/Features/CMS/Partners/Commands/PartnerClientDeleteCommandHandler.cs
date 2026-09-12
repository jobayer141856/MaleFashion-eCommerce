using Cortex.Mediator.Commands;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Contracts.Services;
using MaleFashion.Application.Exceptions;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.Features.CMS.Partners.Commands
{
    public class PartnerClientDeleteCommandHandler : ICommandHandler<PartnerClientDeleteCommand>
    {
        private readonly IApplicationUnitOfWork _unitOfWork;
        private readonly IFileStorageService _fileStorageService;
        private readonly ILogger<PartnerClientDeleteCommandHandler> _logger;

        public PartnerClientDeleteCommandHandler(
            IApplicationUnitOfWork unitOfWork,
            IFileStorageService fileStorageService,
            ILogger<PartnerClientDeleteCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _fileStorageService = fileStorageService;
            _logger = logger;
        }

        public async Task Handle(PartnerClientDeleteCommand command, CancellationToken cancellationToken)
        {
            var partner = await _unitOfWork.PartnerClientRepository.GetByIdAsync(command.Id, cancellationToken);
            if (partner == null)
                throw new NotFoundException($"Partner client with ID '{command.Id}' not found.");

            var logoUrl = partner.LogoUrl;
            await _unitOfWork.PartnerClientRepository.RemoveAsync(command.Id, cancellationToken);
            await _unitOfWork.SaveAsync(cancellationToken);

            if (!string.IsNullOrEmpty(logoUrl))
            {
                try
                {
                    await _fileStorageService.DeleteImageAsync(logoUrl, "partners", cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to delete partner logo '{LogoUrl}' for ID '{Id}'", logoUrl, command.Id);
                }
            }
        }
    }
}
