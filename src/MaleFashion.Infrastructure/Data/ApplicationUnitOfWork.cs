using MaleFashion.Application.Contracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace MaleFashion.Infrastructure.Data
{
    public class ApplicationUnitOfWork : UnitOfWork, IApplicationUnitOfWork
    {

        public ApplicationUnitOfWork(ApplicationDbContext dbContext) : base(dbContext)
        {
        }
    }
}
