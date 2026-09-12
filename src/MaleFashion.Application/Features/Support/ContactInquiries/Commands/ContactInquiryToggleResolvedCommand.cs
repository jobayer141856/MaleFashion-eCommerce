using Cortex.Mediator.Commands;
using System;

namespace MaleFashion.Application.Features.Support.ContactInquiries.Commands
{
    public class ContactInquiryToggleResolvedCommand : ICommand<bool>
    {
        public Guid Id { get; set; }
    }
}

