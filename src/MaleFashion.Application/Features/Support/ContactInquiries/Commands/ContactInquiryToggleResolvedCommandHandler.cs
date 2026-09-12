using Cortex.Mediator.Commands;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Exceptions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.Features.Support.ContactInquiries.Commands
{
    public class ContactInquiryToggleResolvedCommandHandler : ICommandHandler<ContactInquiryToggleResolvedCommand, bool>
    {
        private readonly IApplicationUnitOfWork _unitOfWork;

        public ContactInquiryToggleResolvedCommandHandler(IApplicationUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(ContactInquiryToggleResolvedCommand command, CancellationToken cancellationToken)
        {
            var inquiry = await _unitOfWork.ContactInquiryRepository.GetByIdAsync(command.Id, cancellationToken);
            if (inquiry == null)
                throw new NotFoundException($"Contact inquiry with ID '{command.Id}' not found.");

            inquiry.IsResolved = !inquiry.IsResolved;
            inquiry.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.ContactInquiryRepository.EditAsync(inquiry, cancellationToken);
            await _unitOfWork.SaveAsync(cancellationToken);
            return inquiry.IsResolved;
        }
    }
}

