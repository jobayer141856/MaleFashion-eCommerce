using Cortex.Mediator.Commands;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Exceptions;
using MaleFashion.Domain.Entities.Support;
using MapsterMapper;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.Features.Support.StoreLocations.Commands
{
    public class StoreLocationUpdateCommandHandler : ICommandHandler<StoreLocationUpdateCommand, StoreLocation>
    {
        private readonly IApplicationUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public StoreLocationUpdateCommandHandler(IApplicationUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<StoreLocation> Handle(StoreLocationUpdateCommand command, CancellationToken cancellationToken)
        {
            var isDuplicate = await _unitOfWork.StoreLocationRepository.IsDuplicateTitle(command.Title, command.Id, cancellationToken);
            if (isDuplicate)
                throw new DuplicateDataException("Store location title already exists.");

            var location = await _unitOfWork.StoreLocationRepository.GetByIdAsync(command.Id, cancellationToken);
            if (location == null)
                throw new NotFoundException($"Store location with ID '{command.Id}' not found.");

            location = _mapper.Map(command, location);
            location.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.StoreLocationRepository.EditAsync(location, cancellationToken);
            await _unitOfWork.SaveAsync(cancellationToken);
            return location;
        }
    }
}

