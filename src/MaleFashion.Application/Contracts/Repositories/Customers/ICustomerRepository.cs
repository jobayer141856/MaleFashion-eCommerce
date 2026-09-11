using MaleFashion.Application.Features.Customers.Queries;
using MaleFashion.Domain.Contracts;
using MaleFashion.Domain.Entities.Customers;
using System;
using System.Collections.Generic;
using System.Text;
using static MaleFashion.Domain.Contracts.IRepository;

namespace MaleFashion.Application.Contracts.Repositories.Customers
{
    public interface ICustomerRepository : IRepository<Customer, Guid>
    {
        Task<(IList<Customer>, int, int)> GetPagedCustomers(GetAllCustomersByPagingQuery query, CancellationToken cancellationToken);
        Task<Customer?> GetCustomerDetailsById(Guid id, CancellationToken cancellationToken);
        Task<bool> IsDuplicateEmail(string email, Guid? id, CancellationToken cancellationToken);
    }
}

