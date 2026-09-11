using Cortex.Mediator.Commands;
using MaleFashion.Application.Contracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace MaleFashion.Application.Features.Blogs.Commands
{
    public class BlogCommentDeleteCommandHandler : ICommandHandler<BlogCommentDeleteCommand, bool>
    {
        private readonly IApplicationUnitOfWork _unitOfWork;
        public BlogCommentDeleteCommandHandler(IApplicationUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;
        public async Task<bool> Handle(BlogCommentDeleteCommand command, CancellationToken cancellationToken)
        {
            await _unitOfWork.BlogCommentRepository.RemoveAsync(command.Id, cancellationToken);
            await _unitOfWork.SaveAsync(cancellationToken);
            return true;
        }
    }
}
