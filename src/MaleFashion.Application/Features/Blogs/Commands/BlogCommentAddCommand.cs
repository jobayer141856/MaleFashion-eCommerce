using Cortex.Mediator.Commands;
using MaleFashion.Application.Contracts;
using MaleFashion.Domain.Entities.Blogs;
using MaleFashion.Domain.Utilities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.Features.Blogs.Commands
{
    public class BlogCommentAddCommand : ICommand<BlogComment>
    {
        public Guid BlogPostId { get; set; }
        public Guid? UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string CommentText { get; set; } = string.Empty;
    }

}