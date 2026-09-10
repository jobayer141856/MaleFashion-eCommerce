using Cortex.Mediator.Commands;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Exceptions;
using MaleFashion.Domain.Entities.Catalogs;
using MaleFashion.Domain.Utilities;
using MapsterMapper;
using System;
using System.Collections.Generic;
using System.Text;

namespace MaleFashion.Application.Features.Catalogs.Brands.Commands
{
    public class BrandAddCommandHandler : ICommandHandler<BrandAddCommand, Brand>
    {
        private readonly IApplicationUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public BrandAddCommandHandler(IApplicationUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        public async Task<Brand> Handle(BrandAddCommand command, CancellationToken cancellationToken)
        {
            var isDuplicate = await _unitOfWork.BrandRepository.IsDuplicateBrandName(command.Name, null, cancellationToken);
            if (isDuplicate)
                throw new DuplicateDataException("Brand name already exists.");
            var brand = _mapper.Map<Brand>(command);
            brand.Id = IdentityGenerator.NewSequentialGuid();
            brand.CreatedAt = DateTime.UtcNow;
            await _unitOfWork.BrandRepository.AddAsync(brand, cancellationToken);
            await _unitOfWork.SaveAsync(cancellationToken);
            return brand;
        }
    }
}
