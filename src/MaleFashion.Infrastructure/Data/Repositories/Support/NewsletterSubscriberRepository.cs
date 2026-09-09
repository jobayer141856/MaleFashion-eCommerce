using MaleFashion.Application.Contracts.Repositories.Support;
using MaleFashion.Domain.Entities.Support;
using System;
using System.Collections.Generic;
using System.Text;

namespace MaleFashion.Infrastructure.Data.Repositories.Support
{
    public class NewsletterSubscriberRepository : Repository<NewsletterSubscriber, Guid>, INewsletterSubscriberRepository
    {
        private readonly ApplicationDbContext _context;

        public NewsletterSubscriberRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            _context = dbContext;
        }
    }
}

