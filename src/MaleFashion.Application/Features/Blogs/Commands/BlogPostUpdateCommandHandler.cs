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

            // Update Tags (Differential Synchronization)
            var incomingTagNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (command.Tags != null)
            {
                foreach (var tag in command.Tags)
                {
                    if (!string.IsNullOrWhiteSpace(tag))
                    {
                        incomingTagNames.Add(tag.Trim());
                    }
                }
            }

            // 1. Mark removed tags for deletion
            var tagsToRemove = post.Tags
                .Where(t => !incomingTagNames.Contains(t.Name))
                .ToList();

            foreach (var tag in tagsToRemove)
            {
                post.Tags.Remove(tag);
            }

            // 2. Identify remaining existing tag names
            var existingTagNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var tag in post.Tags)
            {
                existingTagNames.Add(tag.Name);
            }

            // 3. Add only new tags
            foreach (var tagName in incomingTagNames)
            {
                if (!existingTagNames.Contains(tagName))
                {
                    post.Tags.Add(new BlogTag
                    {
                        BlogPostId = post.Id,
                        Name = tagName,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }

            await _unitOfWork.BlogPostRepository.EditAsync(post, cancellationToken);
            await _unitOfWork.SaveAsync(cancellationToken);
            return post;
        }
    }
}
