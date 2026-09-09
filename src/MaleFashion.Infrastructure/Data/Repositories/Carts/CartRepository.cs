using MaleFashion.Application.Contracts.Repositories.Carts;
using MaleFashion.Domain.Entities.Carts;
using System;
using System.Collections.Generic;
using System.Text;

namespace MaleFashion.Infrastructure.Data.Repositories.Carts
{
    public class CartRepository : Repository<Cart, Guid>, ICartRepository
    {
        private readonly ApplicationDbContext _context;

        public CartRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            _context = dbContext;
        }
    }
}

