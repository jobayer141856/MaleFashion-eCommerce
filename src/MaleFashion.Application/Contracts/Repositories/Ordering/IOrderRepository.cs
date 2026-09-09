using MaleFashion.Domain.Contracts;
using MaleFashion.Domain.Entities.Ordering;
using System;
using System.Collections.Generic;
using System.Text;
using static MaleFashion.Domain.Contracts.IRepository;

namespace MaleFashion.Application.Contracts.Repositories.Ordering
{
    public interface IOrderRepository : IRepository<Order, Guid>
    {
    }
}

