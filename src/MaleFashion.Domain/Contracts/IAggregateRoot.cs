using System;
using System.Collections.Generic;
using System.Text;

namespace MaleFashion.Domain.Contracts
{
    public interface IAggregateRoot<TKey>
    {
        TKey Id { get; set; }
    }

}
