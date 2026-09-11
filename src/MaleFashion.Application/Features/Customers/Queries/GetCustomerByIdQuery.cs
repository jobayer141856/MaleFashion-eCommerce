using Cortex.Mediator.Queries;
using MaleFashion.Domain.Entities.Customers;
using System;
using System.Collections.Generic;
using System.Text;

namespace MaleFashion.Application.Features.Customers.Queries
{
    public class GetCustomerByIdQuery : IQuery<Customer?>
    {
        public Guid Id { get; set; }
    }
}
