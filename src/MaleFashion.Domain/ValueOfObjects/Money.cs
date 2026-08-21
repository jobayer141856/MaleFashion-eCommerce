using System;
using System.Collections.Generic;
using System.Text;

namespace MaleFashion.Domain.ValueOfObjects
{
    public record Money(decimal Amount, string Currency = "BDT");
}
