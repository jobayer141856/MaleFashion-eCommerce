using MaleFashion.Domain.Contracts;
using MaleFashion.Domain.Entities.CMS;
using System;
using System.Collections.Generic;
using System.Text;
using static MaleFashion.Domain.Contracts.IRepository;

namespace MaleFashion.Application.Contracts.Repositories.CMS
{
    public interface IPartnerClientRepository : IRepository<PartnerClient, Guid>
    {
    }
}

