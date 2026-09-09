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
    }
}

