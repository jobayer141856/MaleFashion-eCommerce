using Cortex.Mediator.Queries;
using MaleFashion.Application.Contracts;
using MaleFashion.Domain.Entities.Catalogs;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.Features.Catalogs.Products.Queries
{
    public class GetProductByIdQueryHandler : IQueryHandler<GetProductByIdQuery, Product?>
    {
        private readonly IApplicationUnitOfWork _unitOfWork;

        public GetProductByIdQueryHandler(IApplicationUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Product?> Handle(GetProductByIdQuery query, CancellationToken cancellationToken)
        {
            return await _unitOfWork.ProductRepository.GetProductDetailsById(query.Id, cancellationToken);
        }
    }
}

