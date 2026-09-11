using Cortex.Mediator.Queries;
using MaleFashion.Domain.Entities.Customers;
using System;
using System.Collections.Generic;
using System.Text;

namespace MaleFashion.Application.Features.Customers.Queries
{
    public class GetCustomerDetailsByIdQuery : IQuery<Customer?>
    {
        public Guid Id { get; set; }
    }
}
