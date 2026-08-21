using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace MaleFashion.Infrastructure.Identity
{
    public class ApplicationUserRole
        : IdentityUserRole<Guid>
    {
       
    }
}
