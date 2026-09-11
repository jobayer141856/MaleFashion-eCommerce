using Cortex.Mediator.Commands;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Exceptions;
using MaleFashion.Domain.Entities.Blogs;
using MaleFashion.Domain.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.Features.Blogs.Commands
{
    public class BlogPostUpdateCommand : ICommand<BlogPost>
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string AuthorName { get; set; } = "Admin";
        public string? FeaturedImageUrl { get; set; }
        public bool IsPublished { get; set; }
        public List<string> Tags { get; set; } = new();
    }

   
}