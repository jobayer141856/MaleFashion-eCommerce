using System;
using System.Collections.Generic;
using System.Text;

namespace MaleFashion.Application.Contracts
{
    public interface IServerTime
    {
        DateTime DateTime { get; }
    }
}
