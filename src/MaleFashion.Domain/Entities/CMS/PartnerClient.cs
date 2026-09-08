using MaleFashion.Domain.Contracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace MaleFashion.Domain.Entities.CMS
{
    public class PartnerClient : IAggregateRoot<Guid>, IAuditableEntity
    {
        public Guid Id { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public string LogoUrl { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

    }
}
