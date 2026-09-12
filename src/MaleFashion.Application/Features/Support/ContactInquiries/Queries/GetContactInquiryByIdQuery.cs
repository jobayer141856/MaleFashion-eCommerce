using Cortex.Mediator.Queries;
using MaleFashion.Domain.Entities.Support;
using System;

namespace MaleFashion.Application.Features.Support.ContactInquiries.Queries
{
    public class GetContactInquiryByIdQuery : IQuery<ContactInquiry?>
    {
        public Guid Id { get; set; }
    }
}

