using Cortex.Mediator.Queries;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Features.Catalogs.Products.DTOs;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.Features.Catalogs.Products.Queries
{
    public class GetShopProductsQueryHandler : IQueryHandler<GetShopProductsQuery, (IList<ShopProductItemDto> Products, int Total, int TotalDisplay)>
    {
        private readonly IApplicationUnitOfWork _unitOfWork;

        public GetShopProductsQueryHandler(IApplicationUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<(IList<ShopProductItemDto> Products, int Total, int TotalDisplay)> Handle(
            GetShopProductsQuery query, CancellationToken cancellationToken)
        {
            var parameters = new Dictionary<string, object?>
            {
                { "@PageIndex", query.PageIndex < 1 ? 1 : query.PageIndex },
                { "@PageSize", query.PageSize < 1 ? 12 : query.PageSize },
                { "@OrderBy", query.OrderBy ?? "Newest" },
                { "@SearchText", query.SearchText },
                { "@CategoryId", query.CategoryId },
                { "@BrandId", query.BrandId },
                { "@PriceFrom", query.PriceFrom },
                { "@PriceTo", query.PriceTo },
                { "@Size", query.Size },
                { "@Color", query.Color },
                { "@TagName", query.TagName }
            };

            var outParameters = new Dictionary<string, Type>
            {
                { "@Total", typeof(int) },
                { "@TotalDisplay", typeof(int) }
            };

            var (result, outValues) = await _unitOfWork.SqlUtility.QueryWithStoredProcedureAsync<ShopProductItemDto>(
                "GetShopProducts", parameters, outParameters);

            int total = 0;
            if (outValues != null)
            {
                if (outValues.TryGetValue("@Total", out var tVal) && tVal != null && tVal != DBNull.Value)
                    total = Convert.ToInt32(tVal);
                else if (outValues.TryGetValue("Total", out var tVal2) && tVal2 != null && tVal2 != DBNull.Value)
                    total = Convert.ToInt32(tVal2);
            }

            int totalDisplay = 0;
            if (outValues != null)
            {
                if (outValues.TryGetValue("@TotalDisplay", out var tdVal) && tdVal != null && tdVal != DBNull.Value)
                    totalDisplay = Convert.ToInt32(tdVal);
                else if (outValues.TryGetValue("TotalDisplay", out var tdVal2) && tdVal2 != null && tdVal2 != DBNull.Value)
                    totalDisplay = Convert.ToInt32(tdVal2);
            }

            return (result ?? new List<ShopProductItemDto>(), total, totalDisplay);
        }
    }
}

