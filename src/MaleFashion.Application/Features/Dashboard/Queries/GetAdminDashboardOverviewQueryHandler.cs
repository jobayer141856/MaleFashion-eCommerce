using Cortex.Mediator.Queries;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Features.Dashboard.DTOs;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.Features.Dashboard.Queries
{
    public class GetAdminDashboardOverviewQueryHandler : IQueryHandler<GetAdminDashboardOverviewQuery, AdminDashboardDto>
    {
        private readonly IApplicationUnitOfWork _unitOfWork;

        public GetAdminDashboardOverviewQueryHandler(IApplicationUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<AdminDashboardDto> Handle(GetAdminDashboardOverviewQuery query, CancellationToken cancellationToken)
        {
            return await _unitOfWork.OrderRepository.GetDashboardOverviewAsync(cancellationToken);
        }
    }
}

