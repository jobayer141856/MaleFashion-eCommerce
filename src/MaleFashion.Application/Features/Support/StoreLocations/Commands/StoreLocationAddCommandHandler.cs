using Cortex.Mediator.Commands;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Exceptions;
using MaleFashion.Domain.Entities.Support;
using MaleFashion.Domain.Utilities;
using MapsterMapper;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.Features.Support.StoreLocations.Commands
{
    public class StoreLocationAddCommandHandler : ICommandHandler<StoreLocationAddCommand, StoreLocation>
    {
        private readonly IApplicationUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public StoreLocationAddCommandHandler(IApplicationUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<StoreLocation> Handle(StoreLocationAddCommand command, CancellationToken cancellationToken)
        {
            var isDuplicate = await _unitOfWork.StoreLocationRepository.IsDuplicateTitle(command.Title, null, cancellationToken);
            if (isDuplicate)
                throw new DuplicateDataException("Store location title already exists.");

            var location = _mapper.Map<StoreLocation>(command);
            location.Id = IdentityGenerator.NewSequentialGuid();
            location.CreatedAt = DateTime.UtcNow;

            await _unitOfWork.StoreLocationRepository.AddAsync(location, cancellationToken);
            await _unitOfWork.SaveAsync(cancellationToken);
            return location;
        }
    }
}

