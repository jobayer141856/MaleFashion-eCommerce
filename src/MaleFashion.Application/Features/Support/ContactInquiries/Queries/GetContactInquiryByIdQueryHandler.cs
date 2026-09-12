using Cortex.Mediator.Queries;
using MaleFashion.Application.Contracts;
using MaleFashion.Domain.Entities.Support;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.Features.Support.ContactInquiries.Queries
{
    public class GetContactInquiryByIdQueryHandler : IQueryHandler<GetContactInquiryByIdQuery, ContactInquiry?>
    {
        private readonly IApplicationUnitOfWork _unitOfWork;

        public GetContactInquiryByIdQueryHandler(IApplicationUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ContactInquiry?> Handle(GetContactInquiryByIdQuery query, CancellationToken cancellationToken)
        {
            return await _unitOfWork.ContactInquiryRepository.GetByIdAsync(query.Id, cancellationToken);
        }
    }
}

