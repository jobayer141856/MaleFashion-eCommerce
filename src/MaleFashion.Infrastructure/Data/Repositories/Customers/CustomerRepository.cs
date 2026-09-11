using MaleFashion.Application.Contracts.Repositories.Customers;
using MaleFashion.Application.Features.Customers.Queries;
using MaleFashion.Domain.Entities.Customers;
using MaleFashion.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Infrastructure.Data.Repositories.Customers
{
    public class CustomerRepository : Repository<Customer, Guid>, ICustomerRepository
    {
        private readonly ApplicationDbContext _context;

        public CustomerRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            _context = dbContext;
        }

        public async Task<(IList<Customer>, int, int)> GetPagedCustomers(GetAllCustomersByPagingQuery query, CancellationToken cancellationToken)
        {
            var sortText = string.IsNullOrWhiteSpace(query.SortText) ? "CreatedAt desc" : query.SortText;

            return await GetDynamicAsync(
                x => query.SearchText == null ||
                     x.FirstName.Contains(query.SearchText) ||
                     x.LastName.Contains(query.SearchText) ||
                     x.Email.Contains(query.SearchText) ||
                     x.PhoneNumber.Contains(query.SearchText),
                sortText,
                x => x.Include(c => c.Orders),
                query.PageIndex,
                query.PageSize,
                true,
                cancellationToken);
        }

        public async Task<Customer?> GetCustomerDetailsById(Guid id, CancellationToken cancellationToken)
        {
            var list = await GetAsync(
                x => x.Id == id,
                x => x.Include(c => c.Addresses)
                      .Include(c => c.Orders)
                      .Include(c => c.Reviews),
                cancellationToken);

            return list.FirstOrDefault();
        }

        public async Task<bool> IsDuplicateEmail(string email, Guid? id, CancellationToken cancellationToken)
        {
            var cleanEmail = email.Trim().ToLowerInvariant();
            // Duplicate email check applies to registered accounts (Self & Admin), not anonymous guests
            if (!id.HasValue)
                return (await GetCountAsync(x => x.Email == cleanEmail && x.Origin != CustomerOrigin.Guest, cancellationToken)) > 0;
            else
                return (await GetCountAsync(x => x.Email == cleanEmail && x.Id != id.Value && x.Origin != CustomerOrigin.Guest, cancellationToken)) > 0;
        }
    }
}