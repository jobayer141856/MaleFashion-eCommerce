using Cortex.Mediator.Queries;

namespace MaleFashion.Application.Features.Reviews.Queries
{
    public class GetProductReviewStatsQuery : IQuery<(int Total, int Pending, int Approved, double AverageRating)>
    {
    }
}

