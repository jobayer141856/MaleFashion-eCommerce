using MaleFashion.Application.Contracts.Repositories.CMS;
using MaleFashion.Domain.Entities.CMS;
using System;
using System.Collections.Generic;
using System.Text;

namespace MaleFashion.Infrastructure.Data.Repositories.CMS
{
    public class PartnerClientRepository : Repository<PartnerClient, Guid>, IPartnerClientRepository
    {
        private readonly ApplicationDbContext _context;

        public PartnerClientRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            _context = dbContext;
        }
    }
}

