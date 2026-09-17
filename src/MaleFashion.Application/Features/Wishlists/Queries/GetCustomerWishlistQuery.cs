using Cortex.Mediator.Queries;
using MaleFashion.Application.Features.Wishlists.DTOs;
using System;
using System.Collections.Generic;

namespace MaleFashion.Application.Features.Wishlists.Queries
{
    public class GetCustomerWishlistQuery : IQuery<CustomerWishlistDto>
    {
        public Guid? CustomerId { get; set; }
        public List<Guid> GuestProductIds { get; set; } = new();
    }
}

