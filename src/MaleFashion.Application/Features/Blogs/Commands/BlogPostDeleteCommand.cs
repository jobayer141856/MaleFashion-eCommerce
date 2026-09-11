using Cortex.Mediator.Commands;
using System;
using System.Collections.Generic;
using System.Text;

namespace MaleFashion.Application.Features.Blogs.Commands
{
    public class BlogPostDeleteCommand : ICommand<bool>
    {
        public Guid Id { get; set; }
    }
}
