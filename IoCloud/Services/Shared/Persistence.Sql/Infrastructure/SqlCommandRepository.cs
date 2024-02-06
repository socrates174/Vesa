using Microsoft.EntityFrameworkCore;
using IoCloud.Shared.Entity.Abstractions;
using AutoMapper;
using IoCloud.Shared.Persistence.Sql.Abstractions;
using System.Collections;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;

namespace IoCloud.Shared.Persistence.Sql.Infrastructure
{
    public class SqlCommandRepository<TEntity> : SqlCommandRepository<TEntity, Guid>, ISqlCommandRepository<TEntity>
        where TEntity : class
    {
        public SqlCommandRepository(DbContext dbContext, IMapper mapper) : base(dbContext, mapper)
        {
        }
    }

    public class SqlCommandRepository<TEntity, TKey> : ISqlCommandRepository<TEntity, TKey>
        where TEntity : class
    {
        protected readonly DbContext _dbContext;
        protected readonly IMapper _mapper;
        protected bool disposedValue;

        public SqlCommandRepository(DbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public void Add(TEntity entity)
        {
            _dbContext.Set<TEntity>().Add(entity);
        }
        
        public async Task UpdateAsync(TEntity entity, params string[] navigationProperties)
        {
            var attachedEntity = await GetAsync((TKey)GetKeyValue(entity), navigationProperties);
            _mapper.Map(entity, attachedEntity);
        }

        public async Task AddOrUpdateAsync(TEntity entity, params string[] navigationProperties)
        {
            var attachedEntity = await GetAsync((TKey)GetKeyValue(entity), navigationProperties);
            if (attachedEntity != null)
            {
                _mapper.Map(entity, attachedEntity);
            }
            else
            {
                _dbContext.Set<TEntity>().Add(entity);
            }
        }

        private async Task<TEntity> GetAsync(TKey key, params string[] navigationProperties)
        {
            if (typeof(TEntity) is IEntity)
            {
                Expression<Func<TEntity, bool>> filter = x => ((IEntity)x).Id.Equals(key);
                return await GetQuery(filter, navigationProperties).SingleOrDefaultAsync();
            }
            else if (navigationProperties?.Length == 0)
            {
                return await _dbContext.Set<TEntity>().Where(GetByKeyFilter(key)).SingleOrDefaultAsync();
            }
            else
            {
                return await GetQueryableWithNavigationProperties(navigationProperties).Where(GetByKeyFilter(key)).SingleOrDefaultAsync();
            }
        }

        private object GetKeyValue(TEntity entity)
        {
            Type entityType = typeof(TEntity);
            if (entityType is IEntity)
            {
                return ((IEntity)entity).Id;
            }
            else
            {
                IList<string> keyPropertyNames = GetKeyPropertyNames<TEntity>().ToList();
                if (keyPropertyNames.Count() == 1)
                {
                    return entityType.GetProperty(keyPropertyNames.First()).GetValue(entity, null);
                }
                else
                {
                    var keyValues = new List<object>();
                    foreach (var keyPropertyName in keyPropertyNames)
                    {
                        keyValues.Add(entityType.GetProperty(keyPropertyName).GetValue(entity, null));
                    }
                    return keyValues.ToArray();
                }
            }
        }

        private IQueryable<TEntity> GetAll(params string[] navigationProperties) => GetQueryableWithNavigationProperties(navigationProperties);

        private IQueryable<TEntity> GetQuery(Expression<Func<TEntity, bool>> filter, params string[] navigationProperties) => GetAll(navigationProperties).Where(filter);

        private IQueryable<TEntity> GetQueryableWithNavigationProperties(params string[] navigationProperties)
        {
            IQueryable<TEntity> query = _dbContext.Set<TEntity>();
            foreach (string include in navigationProperties)
            {
                query = query.Include(include);
            }
            return query;
        }

        private string GetByKeyFilter(TKey key)
        {
            string filter = "";
            Type entityType = typeof(TEntity);
            IList<string> keyPropertyNames = GetKeyPropertyNames<TEntity>().ToList();
            if (keyPropertyNames.Count() == 1)
            {
                filter = SetPropertyFilter(keyPropertyNames.First(), key);
            }
            else
            {
                int index = 0;
                foreach (var keyPropertyValue in (IEnumerable)key)
                {
                    filter += $"{SetPropertyFilter(keyPropertyNames[index++], keyPropertyValue)} &&";
                }
            }
            return filter;
        }

        private string SetPropertyFilter(string name, object value)
        {
            string filter = "";
            if (value is string || value is Guid)
            {
                filter = $"{name} == \"{value}\"";
            }
            else if (value is DateTimeOffset)
            {
                filter = $"{name} == DateTime.Parse(\"{((DateTimeOffset)value).ToString()}\")";  //TODO
            }
            else if (value is DateTime)
            {
                filter = $"{name} == DateTime.Parse(\"{((DateTime)value).ToShortDateString()}\")";
            }
            else
            {
                filter = $"{name} == {value}";
            }
            return filter;
        }

        public async Task DeleteAsync(TEntity entity)
        {
            var attachedEntity = await GetAsync((TKey)GetKeyValue(entity));
            if (attachedEntity is ISoftDelete)
            {
                ((ISoftDelete)attachedEntity).IsDeleted = true;
            }
            else
            {
                _dbContext.Set<TEntity>().Remove(attachedEntity);
            }
        }

        public async Task DeleteAsync(TKey key)
        {
            var attachedEntity = await GetAsync(key);
            if (attachedEntity is ISoftDelete)
            {
                ((ISoftDelete)attachedEntity).IsDeleted = true;
            }
            else
            {
                _dbContext.Set<TEntity>().Remove(attachedEntity);
            }
        }

        public async Task<int> SaveChangesAsyc(string requestedBy, CancellationToken cancellationToken = default(CancellationToken))
        {
            foreach (var entityEntry in _dbContext.ChangeTracker.Entries())
            {
                if (entityEntry.State == EntityState.Added)
                {
                    if (entityEntry.Entity is IAuditable auditable)
                    {
                        auditable.CreatedBy = requestedBy;
                        auditable.CreatedOn = DateTimeOffset.Now;
                        auditable.UpdatedBy = auditable.CreatedBy;
                        auditable.UpdatedOn = auditable.CreatedOn;
                    }
                }
                else if (entityEntry.State == EntityState.Modified)
                {
                    if (entityEntry.Entity is IAuditable auditable)
                    {
                        auditable.UpdatedBy = requestedBy;
                        auditable.UpdatedOn = DateTimeOffset.Now;
                    }
                }
                else if (entityEntry.State == EntityState.Deleted)
                {
                    if (entityEntry.Entity is IAuditable auditable)
                    {
                        auditable.UpdatedBy = requestedBy;
                        auditable.UpdatedOn = DateTimeOffset.Now;
                    }
                    if (entityEntry.Entity is ISoftDelete softDelete)
                    {
                        softDelete.IsDeleted = true;
                    }
                }
            }
            return await _dbContext.SaveChangesAsync(cancellationToken);
        }

        private IEnumerable<string> GetKeyPropertyNames<TEntity>() where TEntity : class 
            => _dbContext.Model.FindEntityType(typeof(TEntity)).FindPrimaryKey().Properties.Select(x => x.Name);

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~SqlCommandRepository()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
