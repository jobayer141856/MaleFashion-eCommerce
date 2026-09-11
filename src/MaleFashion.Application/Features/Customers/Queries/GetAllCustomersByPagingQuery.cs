using Cortex.Mediator.Queries;
using MaleFashion.Domain.Entities.Customers;
using MaleFashion.Domain.Utilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace MaleFashion.Application.Features.Customers.Queries
{
    public class GetAllCustomersByPagingQuery : DataTables, IQuery<(IList<Customer>, int, int)>
    {
        public string? SearchText { get; set; }
        public string? SortText { get; set; }
    }
}
