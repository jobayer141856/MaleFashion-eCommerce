using MaleFashion.Application.Contracts.Repositories.Ordering;
using MaleFashion.Domain.Entities.Ordering;
using System;
using System.Collections.Generic;
using System.Text;

namespace MaleFashion.Infrastructure.Data.Repositories.Ordering
{
    public class OrderRepository : Repository<Order, Guid>, IOrderRepository
    {
        private readonly ApplicationDbContext _context;

        public OrderRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            _context = dbContext;
        }
    }
}

