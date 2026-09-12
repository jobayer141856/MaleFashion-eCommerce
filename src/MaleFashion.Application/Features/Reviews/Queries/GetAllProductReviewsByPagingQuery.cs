using Cortex.Mediator.Queries;
using MaleFashion.Domain.Entities.Reviews;
using System.Collections.Generic;

namespace MaleFashion.Application.Features.Reviews.Queries
{
    public class GetAllProductReviewsByPagingQuery : IQuery<(IList<ProductReview>, int, int)>
    {
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public string SearchText { get; set; } = string.Empty;
        public string SortText { get; set; } = string.Empty;
        public bool? StatusFilter { get; set; }
        public int? RatingFilter { get; set; }
        public bool? VerifiedOnly { get; set; }
    }
}

