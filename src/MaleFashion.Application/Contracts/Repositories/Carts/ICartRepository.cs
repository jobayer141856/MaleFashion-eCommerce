using MaleFashion.Domain.Contracts;
using MaleFashion.Domain.Entities.Carts;
using System;
using System.Collections.Generic;
using System.Text;
using static MaleFashion.Domain.Contracts.IRepository;

namespace MaleFashion.Application.Contracts.Repositories.Carts
{
    public interface ICartRepository : IRepository<Cart, Guid>
    {
    }
}

