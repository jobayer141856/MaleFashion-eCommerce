using Cortex.Mediator.Commands;
using System;

namespace MaleFashion.Application.Features.Support.ContactInquiries.Commands
{
    public class ContactInquiryDeleteCommand : ICommand
    {
        public Guid Id { get; set; }
    }
}

