using MaleFashion.Application.Contracts.Repositories.Reviews;
using MaleFashion.Domain.Entities.Reviews;
using System;
using System.Collections.Generic;
using System.Text;

namespace MaleFashion.Infrastructure.Data.Repositories.Reviews
{
    public class ProductReviewRepository : Repository<ProductReview, Guid>, IProductReviewRepository
    {
        private readonly ApplicationDbContext _context;

        public ProductReviewRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            _context = dbContext;
        }
    }
}

