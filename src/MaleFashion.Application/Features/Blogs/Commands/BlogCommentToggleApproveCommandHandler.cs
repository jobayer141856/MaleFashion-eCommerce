using Cortex.Mediator.Commands;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace MaleFashion.Application.Features.Blogs.Commands
{
    public class BlogCommentToggleApproveCommandHandler : ICommandHandler<BlogCommentToggleApproveCommand, bool>
    {
        private readonly IApplicationUnitOfWork _unitOfWork;
        public BlogCommentToggleApproveCommandHandler(IApplicationUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;
        public async Task<bool> Handle(BlogCommentToggleApproveCommand command, CancellationToken cancellationToken)
        {
            var comment = await _unitOfWork.BlogCommentRepository.GetByIdAsync(command.Id, cancellationToken);
            if (comment == null) throw new NotFoundException("Comment not found.");
            comment.IsApproved = !comment.IsApproved;
            comment.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.SaveAsync(cancellationToken);
            return comment.IsApproved;
        }
    }
}
