using Cortex.Mediator.Commands;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Exceptions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.Features.Support.StoreLocations.Commands
{
    public class StoreLocationDeleteCommandHandler : ICommandHandler<StoreLocationDeleteCommand>
    {
        private readonly IApplicationUnitOfWork _unitOfWork;

        public StoreLocationDeleteCommandHandler(IApplicationUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(StoreLocationDeleteCommand command, CancellationToken cancellationToken)
        {
            var location = await _unitOfWork.StoreLocationRepository.GetByIdAsync(command.Id, cancellationToken);
            if (location == null)
                throw new NotFoundException($"Store location with ID '{command.Id}' not found.");

            await _unitOfWork.StoreLocationRepository.RemoveAsync(command.Id, cancellationToken);
            await _unitOfWork.SaveAsync(cancellationToken);
        }
    }
}

