using Cortex.Mediator.Commands;
using MaleFashion.Application.Contracts;
using MaleFashion.Domain.Entities.CMS;
using MaleFashion.Domain.Utilities;
using MapsterMapper;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.Features.CMS.Partners.Commands
{
    public class PartnerClientAddCommandHandler : ICommandHandler<PartnerClientAddCommand, PartnerClient>
    {
        private readonly IApplicationUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public PartnerClientAddCommandHandler(IApplicationUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<PartnerClient> Handle(PartnerClientAddCommand command, CancellationToken cancellationToken)
        {
            var partner = _mapper.Map<PartnerClient>(command);
            partner.Id = IdentityGenerator.NewSequentialGuid();
            partner.CreatedAt = DateTime.UtcNow;

            await _unitOfWork.PartnerClientRepository.AddAsync(partner, cancellationToken);
            await _unitOfWork.SaveAsync(cancellationToken);

            return partner;
        }
    }
}

