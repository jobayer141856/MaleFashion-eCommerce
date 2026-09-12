using Cortex.Mediator.Commands;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Exceptions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.Features.Support.ContactInquiries.Commands
{
    public class ContactInquiryDeleteCommandHandler : ICommandHandler<ContactInquiryDeleteCommand>
    {
        private readonly IApplicationUnitOfWork _unitOfWork;

        public ContactInquiryDeleteCommandHandler(IApplicationUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(ContactInquiryDeleteCommand command, CancellationToken cancellationToken)
        {
            var inquiry = await _unitOfWork.ContactInquiryRepository.GetByIdAsync(command.Id, cancellationToken);
            if (inquiry == null)
                throw new NotFoundException($"Contact inquiry with ID '{command.Id}' not found.");

            await _unitOfWork.ContactInquiryRepository.RemoveAsync(command.Id, cancellationToken);
            await _unitOfWork.SaveAsync(cancellationToken);
        }
    }
}

