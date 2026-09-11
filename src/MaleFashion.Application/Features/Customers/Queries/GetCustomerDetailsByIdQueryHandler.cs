using Cortex.Mediator.Queries;
using MaleFashion.Application.Contracts;
using MaleFashion.Domain.Entities.Customers;
using System;
using System.Collections.Generic;
using System.Text;

namespace MaleFashion.Application.Features.Customers.Queries
{
    public class GetCustomerDetailsByIdQueryHandler : IQueryHandler<GetCustomerDetailsByIdQuery, Customer?>
    {
        private readonly IApplicationUnitOfWork _unitOfWork;
        public GetCustomerDetailsByIdQueryHandler(IApplicationUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<Customer?> Handle(GetCustomerDetailsByIdQuery query, CancellationToken cancellationToken)
        {
            return await _unitOfWork.CustomerRepository.GetCustomerDetailsById(query.Id, cancellationToken);
        }
    }
}
