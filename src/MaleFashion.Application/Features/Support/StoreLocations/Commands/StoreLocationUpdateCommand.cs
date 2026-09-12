using Cortex.Mediator.Commands;
using MaleFashion.Domain.Entities.Support;
using System;

namespace MaleFashion.Application.Features.Support.StoreLocations.Commands
{
    public class StoreLocationUpdateCommand : ICommand<StoreLocation>
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? Email { get; set; }
        public int DisplayOrder { get; set; } = 0;
        public bool IsActive { get; set; } = true;
    }
}

