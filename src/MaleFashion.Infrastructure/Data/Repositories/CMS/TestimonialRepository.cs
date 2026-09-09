using MaleFashion.Application.Contracts.Repositories.CMS;
using MaleFashion.Domain.Entities.CMS;
using System;
using System.Collections.Generic;
using System.Text;

namespace MaleFashion.Infrastructure.Data.Repositories.CMS
{
    public class TestimonialRepository : Repository<Testimonial, Guid>, ITestimonialRepository
    {
        private readonly ApplicationDbContext _context;

        public TestimonialRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            _context = dbContext;
        }
    }
}

