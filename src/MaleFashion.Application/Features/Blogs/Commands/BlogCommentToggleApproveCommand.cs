using Cortex.Mediator.Commands;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Exceptions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.Features.Blogs.Commands
{
    public class BlogCommentToggleApproveCommand : ICommand<bool>
    {
        public Guid Id { get; set; }
    }

}