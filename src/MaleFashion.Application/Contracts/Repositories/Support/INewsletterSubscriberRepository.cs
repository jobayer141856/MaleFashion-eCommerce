using MaleFashion.Domain.Contracts;
using MaleFashion.Domain.Entities.Support;
using System;
using System.Collections.Generic;
using System.Text;
using static MaleFashion.Domain.Contracts.IRepository;

namespace MaleFashion.Application.Contracts.Repositories.Support
{
    public interface INewsletterSubscriberRepository : IRepository<NewsletterSubscriber, Guid>
    {
    }
}

