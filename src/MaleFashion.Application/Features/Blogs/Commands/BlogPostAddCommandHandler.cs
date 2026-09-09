using Cortex.Mediator.Commands;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Exceptions;
using MaleFashion.Domain.Entities.Blogs;
using MaleFashion.Domain.Entities.Catalogs;
using MaleFashion.Domain.Utilities;
using MapsterMapper;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace MaleFashion.Application.Features.Blogs.Commands
{
    public class BlogPostAddCommandHandler : ICommandHandler<BlogPostAddCommand, BlogPost>
    {
        private readonly IApplicationUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public BlogPostAddCommandHandler(IApplicationUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public Task<BlogPost> Handle(BlogPostAddCommand command, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
