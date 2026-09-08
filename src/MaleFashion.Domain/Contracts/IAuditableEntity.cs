using System;
using System.Collections.Generic;
using System.Text;

namespace MaleFashion.Domain.Contracts
{
    public interface IAuditableEntity
    {
        DateTime CreatedAt { get; set; }
        DateTime? UpdatedAt { get; set; }

    }
}
