using MaleFashion.Domain.Contracts;
using MaleFashion.Domain.Entities.Promotions;
using System;
using System.Collections.Generic;
using System.Text;
using static MaleFashion.Domain.Contracts.IRepository;

namespace MaleFashion.Application.Contracts.Repositories.Promotions
{
    public interface IDealCountdownRepository : IRepository<DealCountdown, Guid>
    {
    }
}

