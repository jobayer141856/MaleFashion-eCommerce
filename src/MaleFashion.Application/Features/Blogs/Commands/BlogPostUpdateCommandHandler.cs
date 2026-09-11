using Cortex.Mediator.Commands;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Exceptions;
using MaleFashion.Domain.Entities.Blogs;
using MaleFashion.Domain.Utilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace MaleFashion.Application.Features.Blogs.Commands
{
    public class BlogPostUpdateCommandHandler : ICommandHandler<BlogPostUpdateCommand, BlogPost>
    {
        private readonly IApplicationUnitOfWork _unitOfWork;

        public BlogPostUpdateCommandHandler(IApplicationUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<BlogPost> Handle(BlogPostUpdateCommand command, CancellationToken cancellationToken)
        {
            var post = await _unitOfWork.BlogPostRepository.GetBlogPostDetailsById(command.Id, cancellationToken);
            if (post == null) throw new NotFoundException($"Blog post with ID '{command.Id}' not found.");

            post.Title = command.Title.Trim();
            post.Content = command.Content;
            post.AuthorName = command.AuthorName.Trim();
            if (!string.IsNullOrWhiteSpace(command.FeaturedImageUrl))
            {
                post.FeaturedImageUrl = command.FeaturedImageUrl;
            }
            post.IsPublished = command.IsPublished;
            post.UpdatedAt = DateTime.UtcNow;

            // Sync tags
            post.Tags.Clear();
            foreach (var tag in command.Tags)
            {
                if (!string.IsNullOrWhiteSpace(tag))
                {
                    post.Tags.Add(new BlogTag
                    {
                        Id = IdentityGenerator.NewSequentialGuid(),
                        BlogPostId = post.Id,
                        Name = tag.Trim(),
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }

            await _unitOfWork.SaveAsync(cancellationToken);
            return post;
        }
    }
}
