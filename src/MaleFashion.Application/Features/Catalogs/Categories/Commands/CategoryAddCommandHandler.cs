using Cortex.Mediator.Commands;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Exceptions;
using MaleFashion.Application.Features.Catalogs.Categories.Commands;
using MaleFashion.Domain.Entities.Catalogs;
using MaleFashion.Domain.Utilities;
using MapsterMapper;
using System;
using System.Collections.Generic;
using System.Text;

namespace MaleFashion.Application.Features.Catalogs.Categories.Commands
{
    public class CategoryAddCommandHandler : ICommandHandler<CategoryAddCommand, Category>
    {
        private readonly IApplicationUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CategoryAddCommandHandler(IApplicationUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }


        public async Task<Category> Handle(CategoryAddCommand command, CancellationToken cancellationToken)
        {

            var isDuplicateName = await _unitOfWork.CategoryRepository.IsDuplicateCategoryName(command.Name, null,
                cancellationToken);

            if (!isDuplicateName)
            {
                var category = _mapper.Map<Category>(command);

                category.Id = IdentityGenerator.NewSequentialGuid();

                await _unitOfWork.CategoryRepository.AddAsync(category, cancellationToken);
                await _unitOfWork.SaveAsync(cancellationToken);
                return category;
            }
            else
            {
                throw new DuplicateDataException("Category name already exists.");
            }
        }
    }
}
