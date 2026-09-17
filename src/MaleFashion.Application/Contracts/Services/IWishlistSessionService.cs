using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.Contracts.Services
{
    public interface IWishlistSessionService
    {
        Task<(Guid? CustomerId, List<Guid> GuestProductIds)> GetWishlistIdentityAsync(CancellationToken cancellationToken = default);
        List<Guid> GetGuestWishlist();
        void SaveGuestWishlist(List<Guid> productIds);
        bool ToggleGuestWishlist(Guid productId);
        void RemoveFromGuestWishlist(Guid productId);
    }
}

