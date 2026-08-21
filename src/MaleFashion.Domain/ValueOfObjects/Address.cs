using System;
using System.Collections.Generic;
using System.Text;

namespace MaleFashion.Domain.ValueOfObjects
{
    public record Address(
     string StreetAddress,
     string? ApartmentSuite,
     string City,
     string State,
     string PostalCode,
     string Country);
}
