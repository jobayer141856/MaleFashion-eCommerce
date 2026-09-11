using Cortex.Mediator.Queries;
using MaleFashion.Application.Contracts;
using MaleFashion.Domain.Entities.Blogs;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.Features.Blogs.Queries
{
    public class GetBlogPostByIdQueryHandler : IQueryHandler<GetBlogPostByIdQuery, BlogPost?>
    {
        private readonly IApplicationUnitOfWork _unitOfWork;

        public GetBlogPostByIdQueryHandler(IApplicationUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<BlogPost?> Handle(GetBlogPostByIdQuery query, CancellationToken cancellationToken)
        {
            return await _unitOfWork.BlogPostRepository.GetBlogPostDetailsById(query.Id, cancellationToken);
        }
    }
}