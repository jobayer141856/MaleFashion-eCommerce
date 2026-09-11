using Cortex.Mediator.Queries;
using MaleFashion.Application.Contracts;
using MaleFashion.Domain.Entities.Customers;
using System;
using System.Collections.Generic;
using System.Text;

namespace MaleFashion.Application.Features.Customers.Queries
{
    public class GetAllCustomersByPagingQueryHandler : IQueryHandler<GetAllCustomersByPagingQuery, (IList<Customer>, int, int)>
    {
        private readonly IApplicationUnitOfWork _unitOfWork;
        public GetAllCustomersByPagingQueryHandler(IApplicationUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<(IList<Customer>, int, int)> Handle(GetAllCustomersByPagingQuery query, CancellationToken cancellationToken)
        {
            return await _unitOfWork.CustomerRepository.GetPagedCustomers(query, cancellationToken);
        }
    }
}
