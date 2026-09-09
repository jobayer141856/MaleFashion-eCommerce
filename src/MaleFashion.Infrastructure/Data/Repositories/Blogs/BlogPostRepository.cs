using MaleFashion.Application.Contracts.Repositories.Blogs;
using MaleFashion.Application.Contracts.Repositories.Catalogs;
using MaleFashion.Domain.Entities.Blogs;
using MaleFashion.Domain.Entities.Catalogs;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace MaleFashion.Infrastructure.Data.Repositories.Blogs
{
    public class BlogPostRepository : Repository<BlogPost, Guid>, IBlogPostRepository
    {

        private readonly ApplicationDbContext _context;

        public BlogPostRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            _context = dbContext;
        }
       
    }
}
