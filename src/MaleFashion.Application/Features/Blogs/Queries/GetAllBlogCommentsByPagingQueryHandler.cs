using Cortex.Mediator.Queries;
using MaleFashion.Application.Contracts;
using MaleFashion.Domain.Entities.Blogs;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.Features.Blogs.Queries
{
    public class GetAllBlogCommentsByPagingQueryHandler : IQueryHandler<GetAllBlogCommentsByPagingQuery, (IList<BlogComment>, int, int)>
    {
        private readonly IApplicationUnitOfWork _unitOfWork;

        public GetAllBlogCommentsByPagingQueryHandler(IApplicationUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<(IList<BlogComment>, int, int)> Handle(GetAllBlogCommentsByPagingQuery query, CancellationToken cancellationToken)
        {
            return await _unitOfWork.BlogCommentRepository.GetPagedComments(query, cancellationToken);
        }
    }
}