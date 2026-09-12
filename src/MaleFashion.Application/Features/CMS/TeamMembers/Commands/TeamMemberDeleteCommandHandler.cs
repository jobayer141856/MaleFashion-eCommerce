using Cortex.Mediator.Commands;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Contracts.Services;
using MaleFashion.Application.Exceptions;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.Features.CMS.TeamMembers.Commands
{
    public class TeamMemberDeleteCommandHandler : ICommandHandler<TeamMemberDeleteCommand>
    {
        private readonly IApplicationUnitOfWork _unitOfWork;
        private readonly IFileStorageService _fileStorageService;
        private readonly ILogger<TeamMemberDeleteCommandHandler> _logger;

        public TeamMemberDeleteCommandHandler(
            IApplicationUnitOfWork unitOfWork,
            IFileStorageService fileStorageService,
            ILogger<TeamMemberDeleteCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _fileStorageService = fileStorageService;
            _logger = logger;
        }

        public async Task Handle(TeamMemberDeleteCommand command, CancellationToken cancellationToken)
        {
            var member = await _unitOfWork.TeamMemberRepository.GetByIdAsync(command.Id, cancellationToken);
            if (member == null)
                throw new NotFoundException($"Team member with ID '{command.Id}' not found.");

            var photoUrl = member.PhotoUrl;
            await _unitOfWork.TeamMemberRepository.RemoveAsync(command.Id, cancellationToken);
            await _unitOfWork.SaveAsync(cancellationToken);

            if (!string.IsNullOrEmpty(photoUrl))
            {
                try
                {
                    await _fileStorageService.DeleteImageAsync(photoUrl, "team", cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to delete team member photo '{PhotoUrl}' for ID '{Id}'", photoUrl, command.Id);
                }
            }
        }
    }
}

