using Cortex.Mediator.Queries;
using MaleFashion.Domain.Entities.Reviews;
using System;

namespace MaleFashion.Application.Features.Reviews.Queries
{
    public class GetProductReviewByIdQuery : IQuery<ProductReview?>
    {
        public Guid Id { get; set; }

        public GetProductReviewByIdQuery(Guid id)
        {
            Id = id;
        }
    }
}

