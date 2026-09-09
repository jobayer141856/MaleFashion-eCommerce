using MaleFashion.Application.Contracts.Repositories.Promotions;
using MaleFashion.Domain.Entities.Promotions;
using System;
using System.Collections.Generic;
using System.Text;

namespace MaleFashion.Infrastructure.Data.Repositories.Promotions
{
    public class CouponRepository : Repository<Coupon, Guid>, ICouponRepository
    {
        private readonly ApplicationDbContext _context;

        public CouponRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            _context = dbContext;
        }
    }
}

