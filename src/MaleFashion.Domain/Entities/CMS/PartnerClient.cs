using MaleFashion.Domain.Contracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace MaleFashion.Domain.Entities.CMS
{
    public class PartnerClient : IAggregateRoot<Guid>
    {
        public Guid Id { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public string LogoUrl { get; set; } = string.Empty;
    }
}
