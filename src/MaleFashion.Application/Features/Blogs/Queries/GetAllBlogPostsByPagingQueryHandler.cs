using Cortex.Mediator.Queries;
using MaleFashion.Application.Contracts;
using MaleFashion.Domain.Entities.Blogs;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.Features.Blogs.Queries
{
    public class GetAllBlogPostsByPagingQueryHandler : IQueryHandler<GetAllBlogPostsByPagingQuery, (IList<BlogPost>, int, int)>
    {
        private readonly IApplicationUnitOfWork _unitOfWork;

        public GetAllBlogPostsByPagingQueryHandler(IApplicationUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<(IList<BlogPost>, int, int)> Handle(GetAllBlogPostsByPagingQuery query, CancellationToken cancellationToken)
        {
            return await _unitOfWork.BlogPostRepository.GetPagedBlogPosts(query, cancellationToken);
        }
    }
}