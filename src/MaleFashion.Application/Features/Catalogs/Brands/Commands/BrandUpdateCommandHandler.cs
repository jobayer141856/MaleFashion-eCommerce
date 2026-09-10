using Cortex.Mediator.Commands;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Exceptions;
using MaleFashion.Domain.Entities.Catalogs;
using MapsterMapper;
using System;
using System.Collections.Generic;
using System.Text;

namespace MaleFashion.Application.Features.Catalogs.Brands.Commands
{
    public class BrandUpdateCommandHandler : ICommandHandler<BrandUpdateCommand, Brand>
    {
        private readonly IApplicationUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public BrandUpdateCommandHandler(IApplicationUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        public async Task<Brand> Handle(BrandUpdateCommand command, CancellationToken cancellationToken)
        {
            var isDuplicate = await _unitOfWork.BrandRepository.IsDuplicateBrandName(command.Name, command.Id, cancellationToken);
            if (isDuplicate)
                throw new DuplicateDataException("Brand name already exists.");
            var brand = _unitOfWork.BrandRepository.GetById(command.Id);
            if (brand == null)
                throw new NotFoundException($"Brand with ID '{command.Id}' not found.");
            brand = _mapper.Map(command, brand);
            brand.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.BrandRepository.EditAsync(brand, cancellationToken);
            await _unitOfWork.SaveAsync(cancellationToken);
            return brand;
        }
    }
}
