using System;
using System.Collections.Generic;
using System.Text;

namespace MaleFashion.Domain.Enums
{
    public enum CustomerOrigin
    {
        Self = 1,   // Registered by the customer on the storefront
        Admin = 2,  // Created manually by Admin staff in the dashboard
        Guest = 3   // Created automatically during guest checkout
    }
}

