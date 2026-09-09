using MaleFashion.Application.Contracts.Repositories.Customers;
using MaleFashion.Domain.Entities.Customers;
using System;
using System.Collections.Generic;
using System.Text;

namespace MaleFashion.Infrastructure.Data.Repositories.Customers
{
    public class CustomerRepository : Repository<Customer, Guid>, ICustomerRepository
    {
        private readonly ApplicationDbContext _context;

        public CustomerRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            _context = dbContext;
        }
    }
}

