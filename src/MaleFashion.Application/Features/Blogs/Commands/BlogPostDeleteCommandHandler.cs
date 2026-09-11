using Cortex.Mediator.Commands;
using MaleFashion.Application.Contracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace MaleFashion.Application.Features.Blogs.Commands
{
    public class BlogPostDeleteCommandHandler : ICommandHandler<BlogPostDeleteCommand, bool>
    {
        private readonly IApplicationUnitOfWork _unitOfWork;
        public BlogPostDeleteCommandHandler(IApplicationUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;
        public async Task<bool> Handle(BlogPostDeleteCommand command, CancellationToken cancellationToken)
        {
            await _unitOfWork.BlogPostRepository.RemoveAsync(command.Id, cancellationToken);
            await _unitOfWork.SaveAsync(cancellationToken);
            return true;
        }
    }
}
