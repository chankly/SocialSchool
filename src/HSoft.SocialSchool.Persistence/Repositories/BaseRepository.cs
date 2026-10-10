using HSoft.SocialSchool.Domain.Primitives;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace HSoft.SocialSchool.Persistence.Repositories
{
    public abstract class BaseRepository<T> : IBaseRepository<T> where T : class, IAggregateRoot
    {
        protected readonly MyCompanyDbContext _dbContext;

        protected BaseRepository(MyCompanyDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public virtual async Task<T?> GetById(Guid id)
        {
            return await _dbContext.Set<T>().FindAsync(id);
        }

        public virtual async Task<T?> GetByIdReadOnly(Guid id, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Set<T>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        public virtual async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Set<T>()
                .AsNoTracking()
                .AnyAsync(x => x.Id == id, cancellationToken);
        }

        public void Attach(T entity)
        {
            _dbContext.Attach(entity);
        }

        public async Task Add(T entity)
        {
            _dbContext.Add(entity);
            await _dbContext.SaveChangesAsync();
        }

        public Task AddRange(IReadOnlyList<T> entities) =>
            AddRange<T>(entities);

        public async Task AddRange<TEntity>(IReadOnlyList<TEntity> entities, CancellationToken cancellationToken = default) where TEntity : class
        {
            _dbContext.AddRange(entities);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task Update(T entity)
        {
            var entry = _dbContext.Entry(entity);

            if (entry.State == EntityState.Detached)
            {
                // If another instance with the same Id is already tracked (e.g. loaded earlier in the same
                // request scope via a different query), Attach would throw InvalidOperationException.
                // Copy the scalar values onto the tracked instance instead.
                var tracked = _dbContext.Set<T>().Local.FirstOrDefault(e => e.Id == entity.Id);
                if (tracked is not null)
                    _dbContext.Entry(tracked).CurrentValues.SetValues(entity);
                else
                {
                    _dbContext.Attach(entity);
                    _dbContext.Entry(entity).State = EntityState.Modified;
                }
            }
            else
            {
                _dbContext.Entry(entity).State = EntityState.Modified;
            }

            await _dbContext.SaveChangesAsync();
        }

        public async Task Save(T entity) => await Save([entity]);

        public async Task Save(IReadOnlyList<T> entities)
        {
            foreach (var entity in entities)
            {
                var entry = _dbContext.Entry(entity);
                if (entry.State != EntityState.Detached)
                    continue;

                if (BaseRepository<T>.IsNew(entity))
                    _dbContext.Add(entity);
                else
                {
                    _dbContext.Attach(entity);
                    entry.State = EntityState.Modified;
                }
            }

            await _dbContext.SaveChangesAsync();
        }

        public async Task Delete(T entity)
        {
            _dbContext.Remove(entity);
            await _dbContext.SaveChangesAsync();
        }

        private static bool IsNew(T entity) => entity.Id == Guid.Empty;
    }
}