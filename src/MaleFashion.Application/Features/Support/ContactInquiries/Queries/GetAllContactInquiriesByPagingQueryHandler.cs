using Cortex.Mediator.Queries;
using MaleFashion.Application.Contracts;
using MaleFashion.Domain.Entities.Support;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.Features.Support.ContactInquiries.Queries
{
    public class GetAllContactInquiriesByPagingQueryHandler : IQueryHandler<GetAllContactInquiriesByPagingQuery, (IList<ContactInquiry>, int, int)>
    {
        private readonly IApplicationUnitOfWork _unitOfWork;

        public GetAllContactInquiriesByPagingQueryHandler(IApplicationUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<(IList<ContactInquiry>, int, int)> Handle(GetAllContactInquiriesByPagingQuery query, CancellationToken cancellationToken)
        {
            return await _unitOfWork.ContactInquiryRepository.GetPagedContactInquiries(query, cancellationToken);
        }
    }
}

