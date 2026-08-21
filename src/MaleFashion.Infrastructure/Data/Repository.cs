using MaleFashion.Domain.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Collections.Generic;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Text;
using static MaleFashion.Domain.Contracts.IRepository;

namespace MaleFashion.Infrastructure.Data
{
    public abstract class Repository<TAggregateRoot, TKey> : IRepository<TAggregateRoot, TKey>, IDisposable
        where TAggregateRoot : class, IAggregateRoot<TKey>
        where TKey : IComparable
    {
        private DbContext _dbContext;
        private DbSet<TAggregateRoot> _dbSet;

        public Repository(DbContext context)
        {
            _dbContext = context;
            _dbSet = _dbContext.Set<TAggregateRoot>();
        }

        public virtual async Task AddAsync(TAggregateRoot entity, CancellationToken cancellationToken)
        {
            await _dbSet.AddAsync(entity, cancellationToken);
        }

        public virtual async Task RemoveAsync(TKey id, CancellationToken cancellationToken)
        {
            var entityToDelete = await _dbSet.FindAsync(id, cancellationToken);
            await RemoveAsync(entityToDelete, cancellationToken);
        }

        public virtual async Task RemoveAsync(TAggregateRoot entityToDelete, CancellationToken cancellationToken)
        {
            await Task.Run(() =>
            {
                if (_dbContext.Entry(entityToDelete).State == EntityState.Detached)
                {
                    _dbSet.Attach(entityToDelete);
                }
                _dbSet.Remove(entityToDelete);
            }, cancellationToken);
        }

        public virtual async Task RemoveAsync(Expression<Func<TAggregateRoot, bool>>? filter, CancellationToken cancellationToken)
        {
            await Task.Run(() =>
            {
                _dbSet.RemoveRange(_dbSet.Where(filter ?? (e => true)));
            }, cancellationToken);
        }

        public virtual async Task EditAsync(TAggregateRoot entityToUpdate, CancellationToken cancellationToken)
        {
            await Task.Run(() =>
            {
                _dbSet.Attach(entityToUpdate);
                _dbContext.Entry(entityToUpdate).State = EntityState.Modified;
            }, cancellationToken);
        }

        public virtual async Task<TAggregateRoot?> GetByIdAsync(TKey id, CancellationToken cancellationToken)
        {
            return await _dbSet.FindAsync(id, cancellationToken);
        }

        public virtual async Task<int> GetCountAsync(Expression<Func<TAggregateRoot, bool>>? filter = null, CancellationToken cancellationToken = default)
        {
            IQueryable<TAggregateRoot> query = _dbSet;
            int count;

            if (filter is not null)
                count = await query.CountAsync(filter, cancellationToken);
            else
                count = await query.CountAsync(cancellationToken);

            return count;
        }

        public virtual int GetCount(Expression<Func<TAggregateRoot, bool>>? filter = null)
        {
            IQueryable<TAggregateRoot> query = _dbSet;
            int count;

            if (filter is not null)
                count = query.Count(filter);
            else
                count = query.Count();

            return count;
        }

        public virtual async Task<IList<TAggregateRoot>> GetAsync(Expression<Func<TAggregateRoot, bool>> filter,
            Func<IQueryable<TAggregateRoot>, IIncludableQueryable<TAggregateRoot, object>>? include = null, CancellationToken cancellationToken = default)
        {
            IQueryable<TAggregateRoot> query = _dbSet;

            if (filter != null)
            {
                query = query.Where(filter);
            }

            if (include != null)
                query = include(query);

            return await query.ToListAsync(cancellationToken);
        }

        public virtual async Task<IList<TAggregateRoot>> GetAllAsync(CancellationToken cancellationToken)
        {
            IQueryable<TAggregateRoot> query = _dbSet;
            return await query.ToListAsync(cancellationToken);
        }

        public virtual async Task<(IList<TAggregateRoot> data, int total, int totalDisplay)> GetAsync(
            Expression<Func<TAggregateRoot, bool>>? filter = null,
            Func<IQueryable<TAggregateRoot>, IOrderedQueryable<TAggregateRoot>> orderBy = null,
            Func<IQueryable<TAggregateRoot>, IIncludableQueryable<TAggregateRoot, object>> include = null,
            int pageIndex = 1,
            int pageSize = 10,
            bool isTrackingOff = false,
            CancellationToken cancellationToken = default)
        {
            IQueryable<TAggregateRoot> query = _dbSet;
            var total = query.Count();
            var totalDisplay = query.Count();

            if (filter != null)
            {
                query = query.Where(filter);
                totalDisplay = query.Count();
            }

            if (include != null)
                query = include(query);

            IList<TAggregateRoot> data;

            if (orderBy != null)
            {
                var result = orderBy(query).Skip((pageIndex - 1) * pageSize).Take(pageSize);

                if (isTrackingOff)
                    data = await result.AsNoTracking().ToListAsync(cancellationToken);
                else
                    data = await result.ToListAsync(cancellationToken);
            }
            else
            {
                var result = query.Skip((pageIndex - 1) * pageSize).Take(pageSize);

                if (isTrackingOff)
                    data = await result.AsNoTracking().ToListAsync(cancellationToken);
                else
                    data = await result.ToListAsync(cancellationToken);
            }

            return (data, total, totalDisplay);
        }

        public virtual async Task<(IList<TAggregateRoot> data, int total, int totalDisplay)> GetDynamicAsync(
            Expression<Func<TAggregateRoot, bool>>? filter = null,
            string? orderBy = null,
            Func<IQueryable<TAggregateRoot>, IIncludableQueryable<TAggregateRoot, object>> include = null,
            int pageIndex = 1,
            int pageSize = 10,
            bool isTrackingOff = false,
            CancellationToken cancellationToken = default)
        {
            IQueryable<TAggregateRoot> query = _dbSet;
            var total = query.Count();
            var totalDisplay = query.Count();

            if (filter is not null)
            {
                query = query.Where(filter);
                totalDisplay = query.Count();
            }

            if (include is not null)
                query = include(query);

            IList<TAggregateRoot> data;

            if (orderBy is not null)
            {
                var result = query.OrderBy(orderBy).Skip((pageIndex - 1) * pageSize).Take(pageSize);

                if (isTrackingOff)
                    data = await result.AsNoTracking().ToListAsync(cancellationToken);
                else
                    data = await result.ToListAsync(cancellationToken);
            }
            else
            {
                var result = query.Skip((pageIndex - 1) * pageSize).Take(pageSize);

                if (isTrackingOff)
                    data = await result.AsNoTracking().ToListAsync(cancellationToken);
                else
                    data = await result.ToListAsync(cancellationToken);
            }

            return (data, total, totalDisplay);
        }

        public virtual async Task<IList<TAggregateRoot>> GetAsync(
            Expression<Func<TAggregateRoot, bool>> filter = null,
            Func<IQueryable<TAggregateRoot>, IOrderedQueryable<TAggregateRoot>> orderBy = null,
            Func<IQueryable<TAggregateRoot>, IIncludableQueryable<TAggregateRoot, object>> include = null,
            bool isTrackingOff = false,
            CancellationToken cancellationToken = default)
        {
            IQueryable<TAggregateRoot> query = _dbSet;

            if (filter != null)
            {
                query = query.Where(filter);
            }

            if (include != null)
                query = include(query);

            if (orderBy != null)
            {
                var result = orderBy(query);

                if (isTrackingOff)
                    return await result.AsNoTracking().ToListAsync(cancellationToken);
                else
                    return await result.ToListAsync(cancellationToken);
            }
            else
            {
                if (isTrackingOff)
                    return await query.AsNoTracking().ToListAsync(cancellationToken);
                else
                    return await query.ToListAsync(cancellationToken);
            }
        }

        public virtual async Task<IList<TAggregateRoot>> GetDynamicAsync(
            Expression<Func<TAggregateRoot, bool>> filter = null,
            string orderBy = null,
            Func<IQueryable<TAggregateRoot>, IIncludableQueryable<TAggregateRoot, object>> include = null,
            bool isTrackingOff = false,
            CancellationToken cancellationToken = default)
        {
            IQueryable<TAggregateRoot> query = _dbSet;

            if (filter != null)
            {
                query = query.Where(filter);
            }

            if (include != null)
                query = include(query);

            if (orderBy != null)
            {
                var result = query.OrderBy(orderBy);

                if (isTrackingOff)
                    return await result.AsNoTracking().ToListAsync(cancellationToken);
                else
                    return await result.ToListAsync(cancellationToken);
            }
            else
            {
                if (isTrackingOff)
                    return await query.AsNoTracking().ToListAsync(cancellationToken);
                else
                    return await query.ToListAsync(cancellationToken);
            }
        }

        public virtual void Add(TAggregateRoot entity)
        {
            _dbSet.Add(entity);
        }

        public virtual void Remove(TKey id)
        {
            var entityToDelete = _dbSet.Find(id);
            Remove(entityToDelete);
        }

        public virtual void Update(TAggregateRoot entity)
        {
            _dbSet.Update(entity);
        }

        public virtual void Remove(TAggregateRoot entityToDelete)
        {
            if (_dbContext.Entry(entityToDelete).State == EntityState.Detached)
            {
                _dbSet.Attach(entityToDelete);
            }
            _dbSet.Remove(entityToDelete);
        }

        public virtual void Remove(Expression<Func<TAggregateRoot, bool>> filter)
        {
            _dbSet.RemoveRange(_dbSet.Where(filter));
        }

        public virtual void Edit(TAggregateRoot entityToUpdate)
        {
            if (!_dbSet.Local.Any(x => x == entityToUpdate))
            {
                _dbSet.Attach(entityToUpdate);
                _dbContext.Entry(entityToUpdate).State = EntityState.Modified;
            }
        }

        public virtual IList<TAggregateRoot> Get(Expression<Func<TAggregateRoot, bool>> filter,
            Func<IQueryable<TAggregateRoot>, IIncludableQueryable<TAggregateRoot, object>> include = null)
        {
            IQueryable<TAggregateRoot> query = _dbSet;

            if (filter != null)
            {
                query = query.Where(filter);
            }

            if (include != null)
                query = include(query);

            return query.ToList();
        }

        public virtual IList<TAggregateRoot> GetAll()
        {
            IQueryable<TAggregateRoot> query = _dbSet;
            return query.ToList();
        }

        public virtual TAggregateRoot GetById(TKey id)
        {
            return _dbSet.Find(id);
        }

        public virtual (IList<TAggregateRoot> data, int total, int totalDisplay) Get(
            Expression<Func<TAggregateRoot, bool>> filter = null,
            Func<IQueryable<TAggregateRoot>, IOrderedQueryable<TAggregateRoot>> orderBy = null,
            Func<IQueryable<TAggregateRoot>, IIncludableQueryable<TAggregateRoot, object>> include = null,
            int pageIndex = 1, int pageSize = 10, bool isTrackingOff = false)
        {
            IQueryable<TAggregateRoot> query = _dbSet;
            var total = query.Count();
            var totalDisplay = query.Count();

            if (filter != null)
            {
                query = query.Where(filter);
                totalDisplay = query.Count();
            }

            if (include != null)
                query = include(query);

            if (orderBy != null)
            {
                var result = orderBy(query).Skip((pageIndex - 1) * pageSize).Take(pageSize);
                if (isTrackingOff)
                    return (result.AsNoTracking().ToList(), total, totalDisplay);
                else
                    return (result.ToList(), total, totalDisplay);
            }
            else
            {
                var result = query.Skip((pageIndex - 1) * pageSize).Take(pageSize);
                if (isTrackingOff)
                    return (result.AsNoTracking().ToList(), total, totalDisplay);
                else
                    return (result.ToList(), total, totalDisplay);
            }
        }

        public virtual (IList<TAggregateRoot> data, int total, int totalDisplay) GetDynamic(
            Expression<Func<TAggregateRoot, bool>> filter = null,
            string? orderBy = null,
            Func<IQueryable<TAggregateRoot>, IIncludableQueryable<TAggregateRoot, object>> include = null,
            int pageIndex = 1, int pageSize = 10, bool isTrackingOff = false)
        {
            IQueryable<TAggregateRoot> query = _dbSet;
            var total = query.Count();
            var totalDisplay = query.Count();

            if (filter != null)
            {
                query = query.Where(filter);
                totalDisplay = query.Count();
            }

            if (include != null)
                query = include(query);

            if (orderBy != null)
            {
                var result = query.OrderBy(orderBy).Skip((pageIndex - 1) * pageSize).Take(pageSize);
                if (isTrackingOff)
                    return (result.AsNoTracking().ToList(), total, totalDisplay);
                else
                    return (result.ToList(), total, totalDisplay);
            }
            else
            {
                var result = query.Skip((pageIndex - 1) * pageSize).Take(pageSize);
                if (isTrackingOff)
                    return (result.AsNoTracking().ToList(), total, totalDisplay);
                else
                    return (result.ToList(), total, totalDisplay);
            }
        }

        public virtual IList<TAggregateRoot> Get(Expression<Func<TAggregateRoot, bool>> filter = null,
            Func<IQueryable<TAggregateRoot>, IOrderedQueryable<TAggregateRoot>> orderBy = null,
            Func<IQueryable<TAggregateRoot>, IIncludableQueryable<TAggregateRoot, object>> include = null,
            bool isTrackingOff = false)
        {
            IQueryable<TAggregateRoot> query = _dbSet;

            if (filter != null)
            {
                query = query.Where(filter);
            }

            if (include != null)
                query = include(query);

            if (orderBy != null)
            {
                var result = orderBy(query);

                if (isTrackingOff)
                    return result.AsNoTracking().ToList();
                else
                    return result.ToList();
            }
            else
            {
                if (isTrackingOff)
                    return query.AsNoTracking().ToList();
                else
                    return query.ToList();
            }
        }

        public virtual IList<TAggregateRoot> GetDynamic(Expression<Func<TAggregateRoot, bool>> filter = null,
            string orderBy = null,
            Func<IQueryable<TAggregateRoot>, IIncludableQueryable<TAggregateRoot, object>> include = null,
            bool isTrackingOff = false)
        {
            IQueryable<TAggregateRoot> query = _dbSet;

            if (filter != null)
            {
                query = query.Where(filter);
            }

            if (include != null)
                query = include(query);

            if (orderBy != null)
            {
                var result = query.OrderBy(orderBy);

                if (isTrackingOff)
                    return result.AsNoTracking().ToList();
                else
                    return result.ToList();
            }
            else
            {
                if (isTrackingOff)
                    return query.AsNoTracking().ToList();
                else
                    return query.ToList();
            }
        }

        public async Task<IEnumerable<TResult>> GetAsync<TResult>(Expression<Func<TAggregateRoot, TResult>>? selector,
            Expression<Func<TAggregateRoot, bool>>? predicate = null,
            Func<IQueryable<TAggregateRoot>, IOrderedQueryable<TAggregateRoot>>? orderBy = null,
            Func<IQueryable<TAggregateRoot>, IIncludableQueryable<TAggregateRoot, object>>? include = null,
            bool disableTracking = true,
            CancellationToken cancellationToken = default) where TResult : class
        {
            var query = _dbSet.AsQueryable();
            if (disableTracking) query.AsNoTracking();
            if (include is not null) query = include(query);
            if (predicate is not null) query = query.Where(predicate);
            return orderBy is not null
                ? await orderBy(query).Select(selector!).ToListAsync(cancellationToken)
                : await query.Select(selector!).ToListAsync(cancellationToken);
        }

        public async Task<TResult> SingleOrDefaultAsync<TResult>(Expression<Func<TAggregateRoot, TResult>>? selector,
            Expression<Func<TAggregateRoot, bool>>? predicate = null,
            Func<IQueryable<TAggregateRoot>, IOrderedQueryable<TAggregateRoot>>? orderBy = null,
            Func<IQueryable<TAggregateRoot>, IIncludableQueryable<TAggregateRoot, object>>? include = null,
            bool disableTracking = true,
            CancellationToken cancellationToken = default)
        {
            var query = _dbSet.AsQueryable();
            if (disableTracking) query.AsNoTracking();
            if (include is not null) query = include(query);
            if (predicate is not null) query = query.Where(predicate);
            return (orderBy is not null
                ? await orderBy(query).Select(selector!).FirstOrDefaultAsync(cancellationToken)
                : await query.Select(selector!).FirstOrDefaultAsync(cancellationToken))!;
        }

        public void Dispose()
        {
            _dbContext.Dispose();
        }
    }

}
