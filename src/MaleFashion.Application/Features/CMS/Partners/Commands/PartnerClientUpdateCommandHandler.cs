using Cortex.Mediator.Commands;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Exceptions;
using MaleFashion.Domain.Entities.CMS;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.Features.CMS.Partners.Commands
{
    public class PartnerClientUpdateCommandHandler : ICommandHandler<PartnerClientUpdateCommand, PartnerClient>
    {
        private readonly IApplicationUnitOfWork _unitOfWork;

        public PartnerClientUpdateCommandHandler(IApplicationUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<PartnerClient> Handle(PartnerClientUpdateCommand command, CancellationToken cancellationToken)
        {
            var partner = await _unitOfWork.PartnerClientRepository.GetByIdAsync(command.Id, cancellationToken);
            if (partner == null)
                throw new NotFoundException($"Partner client with ID '{command.Id}' not found.");

            partner.ClientName = command.ClientName;
            if (!string.IsNullOrEmpty(command.LogoUrl))
            {
                partner.LogoUrl = command.LogoUrl;
            }
            partner.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.PartnerClientRepository.EditAsync(partner, cancellationToken);
            await _unitOfWork.SaveAsync(cancellationToken);

            return partner;
        }
    }
}

