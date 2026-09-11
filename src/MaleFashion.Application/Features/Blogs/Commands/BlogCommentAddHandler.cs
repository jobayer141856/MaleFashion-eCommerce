using Cortex.Mediator.Commands;
using MaleFashion.Application.Contracts;
using MaleFashion.Domain.Entities.Blogs;
using MaleFashion.Domain.Utilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace MaleFashion.Application.Features.Blogs.Commands
{
    public class BlogCommentAddCommandHandler : ICommandHandler<BlogCommentAddCommand, BlogComment>
    {
        private readonly IApplicationUnitOfWork _unitOfWork;
        public BlogCommentAddCommandHandler(IApplicationUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;
        public async Task<BlogComment> Handle(BlogCommentAddCommand command, CancellationToken cancellationToken)
        {
            var comment = new BlogComment
            {
                Id = IdentityGenerator.NewSequentialGuid(),
                BlogPostId = command.BlogPostId,
                UserId = command.UserId,
                Name = command.Name.Trim(),
                Email = command.Email.Trim().ToLowerInvariant(),
                Phone = command.Phone?.Trim(),
                CommentText = command.CommentText.Trim(),
                IsApproved = false, // Always requires admin moderation
                CreatedAt = DateTime.UtcNow
            };
            await _unitOfWork.BlogCommentRepository.AddAsync(comment, cancellationToken);
            await _unitOfWork.SaveAsync(cancellationToken);
            return comment;
        }
    }
}
