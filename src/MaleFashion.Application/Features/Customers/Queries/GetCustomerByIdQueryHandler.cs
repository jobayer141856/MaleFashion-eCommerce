using Cortex.Mediator.Queries;
using MaleFashion.Application.Contracts;
using MaleFashion.Domain.Entities.Customers;
using System;
using System.Collections.Generic;
using System.Text;

namespace MaleFashion.Application.Features.Customers.Queries
{
    public class GetCustomerByIdQueryHandler : IQueryHandler<GetCustomerByIdQuery, Customer?>
    {
        private readonly IApplicationUnitOfWork _unitOfWork;
        public GetCustomerByIdQueryHandler(IApplicationUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<Customer?> Handle(GetCustomerByIdQuery query, CancellationToken cancellationToken)
        {
            return await _unitOfWork.CustomerRepository.GetByIdAsync(query.Id, cancellationToken);
        }
    }
}
