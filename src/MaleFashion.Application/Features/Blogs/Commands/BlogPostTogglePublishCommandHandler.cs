using Cortex.Mediator.Commands;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace MaleFashion.Application.Features.Blogs.Commands
{
    public class BlogPostTogglePublishCommandHandler : ICommandHandler<BlogPostTogglePublishCommand, bool>
    {
        private readonly IApplicationUnitOfWork _unitOfWork;
        public BlogPostTogglePublishCommandHandler(IApplicationUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;
        public async Task<bool> Handle(BlogPostTogglePublishCommand command, CancellationToken cancellationToken)
        {
            var post = await _unitOfWork.BlogPostRepository.GetByIdAsync(command.Id, cancellationToken);
            if (post == null) throw new NotFoundException("Blog post not found.");
            post.IsPublished = !post.IsPublished;
            post.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.SaveAsync(cancellationToken);
            return post.IsPublished;
        }
    }
}
