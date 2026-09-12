using Cortex.Mediator.Commands;
using MaleFashion.Application.Contracts;
using MaleFashion.Domain.Entities.Support;
using MaleFashion.Domain.Utilities;
using MapsterMapper;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.Features.Support.ContactInquiries.Commands
{
    public class ContactInquiryAddCommandHandler : ICommandHandler<ContactInquiryAddCommand, ContactInquiry>
    {
        private readonly IApplicationUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ContactInquiryAddCommandHandler(IApplicationUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ContactInquiry> Handle(ContactInquiryAddCommand command, CancellationToken cancellationToken)
        {
            var inquiry = _mapper.Map<ContactInquiry>(command);
            inquiry.Id = IdentityGenerator.NewSequentialGuid();
            inquiry.SubmittedAt = DateTime.UtcNow;
            inquiry.IsResolved = false;
            inquiry.CreatedAt = DateTime.UtcNow;

            await _unitOfWork.ContactInquiryRepository.AddAsync(inquiry, cancellationToken);
            await _unitOfWork.SaveAsync(cancellationToken);
            return inquiry;
        }
    }
}

