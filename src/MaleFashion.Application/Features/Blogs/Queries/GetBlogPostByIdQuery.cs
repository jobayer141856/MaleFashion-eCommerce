using Cortex.Mediator.Queries;
using MaleFashion.Domain.Entities.Blogs;
using System;

namespace MaleFashion.Application.Features.Blogs.Queries
{
    public class GetBlogPostByIdQuery : IQuery<BlogPost?>
    {
        public Guid Id { get; set; }
    }
}