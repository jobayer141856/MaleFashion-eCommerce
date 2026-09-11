using Cortex.Mediator.Commands;
using System;
using System.Collections.Generic;
using System.Text;

namespace MaleFashion.Application.Features.Customers.Commands
{
    public class CustomerDeleteCommand : ICommand
    {
        public Guid Id { get; set; }
    }
}
