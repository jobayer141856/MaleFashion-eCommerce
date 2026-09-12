using Cortex.Mediator.Commands;
using MaleFashion.Domain.Entities.Support;
using System;

namespace MaleFashion.Application.Features.Support.ContactInquiries.Commands
{
    public class ContactInquiryAddCommand : ICommand<ContactInquiry>
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}

