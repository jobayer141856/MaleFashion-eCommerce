using System;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.Contracts.Services
{
    public interface ICartSessionService
    {
        Task<(Guid? CustomerId, string SessionId)> GetCartIdentityAsync(CancellationToken cancellationToken = default);
    }
}

