using MaleFashion.Application.Contracts.Repositories.Support;
using MaleFashion.Domain.Entities.Support;
using System;
using System.Collections.Generic;
using System.Text;

namespace MaleFashion.Infrastructure.Data.Repositories.Support
{
    public class ContactInquiryRepository : Repository<ContactInquiry, Guid>, IContactInquiryRepository
    {
        private readonly ApplicationDbContext _context;

        public ContactInquiryRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            _context = dbContext;
        }
    }
}

