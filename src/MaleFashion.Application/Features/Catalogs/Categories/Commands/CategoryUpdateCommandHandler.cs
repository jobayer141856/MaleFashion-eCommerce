using Cortex.Mediator.Commands;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Contracts.Services;
using MaleFashion.Application.Exceptions;
using MaleFashion.Domain.Entities.Catalogs;
using MapsterMapper;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace MaleFashion.Application.Features.Catalogs.Categories.Commands
{
    public class CategoryUpdateCommandHandler : ICommandHandler<CategoryUpdateCommand, Category>
    {
        private readonly IApplicationUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CategoryUpdateCommandHandler(IApplicationUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Category> Handle(CategoryUpdateCommand command, CancellationToken cancellationToken)
        {
            var isDuplicateName = await _unitOfWork.CategoryRepository.IsDuplicateCategoryName(command.Name, command.Id,
                cancellationToken);

            if (!isDuplicateName)
            {
                var category = _unitOfWork.CategoryRepository.GetById(command.Id);
                category = _mapper.Map(command, category);

                category.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.CategoryRepository.EditAsync(category, cancellationToken);
                await _unitOfWork.SaveAsync(cancellationToken);

                return category;
            }
            else
                throw new DuplicateDataException("Category name is duplicate");
        }
    }
}
