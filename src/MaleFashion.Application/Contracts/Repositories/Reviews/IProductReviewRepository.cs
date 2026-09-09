using MaleFashion.Domain.Contracts;
using MaleFashion.Domain.Entities.Reviews;
using System;
using System.Collections.Generic;
using System.Text;
using static MaleFashion.Domain.Contracts.IRepository;

namespace MaleFashion.Application.Contracts.Repositories.Reviews
{
    public interface IProductReviewRepository : IRepository<ProductReview, Guid>
    {
    }
}

