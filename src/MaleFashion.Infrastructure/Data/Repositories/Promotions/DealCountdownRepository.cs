using MaleFashion.Application.Contracts.Repositories.Promotions;
using MaleFashion.Domain.Entities.Promotions;
using System;
using System.Collections.Generic;
using System.Text;

namespace MaleFashion.Infrastructure.Data.Repositories.Promotions
{
    public class DealCountdownRepository : Repository<DealCountdown, Guid>, IDealCountdownRepository
    {
        private readonly ApplicationDbContext _context;

        public DealCountdownRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            _context = dbContext;
        }
    }
}

