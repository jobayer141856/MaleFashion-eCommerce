using Cortex.Mediator.Commands;
using MaleFashion.Application.Contracts;
using MaleFashion.Domain.Entities.Blogs;
using MaleFashion.Domain.Utilities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.Features.Blogs.Commands
{
    public class BlogPostAddCommandHandler : ICommandHandler<BlogPostAddCommand, BlogPost>
    {
        private readonly IApplicationUnitOfWork _unitOfWork;

        public BlogPostAddCommandHandler(IApplicationUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<BlogPost> Handle(BlogPostAddCommand command, CancellationToken cancellationToken)
        {
            var post = new BlogPost
            {
                Id = IdentityGenerator.NewSequentialGuid(),
                Title = command.Title.Trim(),
                Content = command.Content,
                AuthorName = string.IsNullOrWhiteSpace(command.AuthorName) ? "Admin" : command.AuthorName.Trim(),
                FeaturedImageUrl = command.FeaturedImageUrl,
                IsPublished = command.IsPublished,
                PublishedDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

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

            await _unitOfWork.BlogPostRepository.AddAsync(post, cancellationToken);
            await _unitOfWork.SaveAsync(cancellationToken);
            return post;
        }
    }
}