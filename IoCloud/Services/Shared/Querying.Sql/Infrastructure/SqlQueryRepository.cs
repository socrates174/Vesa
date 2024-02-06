using Microsoft.EntityFrameworkCore;
using IoCloud.Shared.Entity.Abstractions;
using IoCloud.Shared.Querying.Abstractions;
using IoCloud.Shared.Querying.Sql.Abstractions;
using System.Collections;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;

namespace IoCloud.Shared.Querying.Sql.Infrastructure
{
    public class SqlQueryRepository<TEntity> : SqlQueryRepository<TEntity, Guid>, IQueryExistence<TEntity>
        where TEntity : class
    {
        public SqlQueryRepository(DbContext context) : base(context)
        {
        }
    }

    public class SqlQueryRepository<TEntity, TKey> : ISqlQueryRepository<TEntity, TKey>, IQueryExistence<TEntity, TKey>
        where TEntity : class
    {
        protected readonly DbContext _dbContext;
        private bool disposedValue;

        public SqlQueryRepository(DbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public SqlQueryRepository()
        {
        }

        public async Task<bool> ExistsAsync(TKey key)
        {
            var entity = await GetAsync(key);
            return entity != null;
        }

        public object GetKeyValue(TEntity entity)
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

        public async Task<TEntity> GetAsync(TKey key, params string[] navigationProperties)
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

        public IQueryable<TEntity> GetAll(params string[] navigationProperties)
        {
            if (!typeof(ISoftDelete).IsAssignableFrom(typeof(TEntity)))
            {
                return GetQueryableWithNavigationProperties(navigationProperties);
            }
            else
            {
                return GetQueryableWithNavigationProperties(navigationProperties).Where(entity => !((ISoftDelete)entity).IsDeleted);
            }

        }

        public IQueryable<TEntity> GetQuery(Expression<Func<TEntity, bool>> filter, params string[] navigationProperties) => GetAll(navigationProperties).Where(filter);

        public IQueryable<TEntity> GetQuery(Expression<Func<TEntity, bool>> filter, int pageNumber, int pageSize, params string[] navigationProperties)
            => GetQuery(filter, navigationProperties).Skip(pageSize * (pageNumber - 1)).Take(pageSize);

        public IQueryable<TEntity> GetQuery
        (
            Expression<Func<TEntity, bool>> filter,
            OrderByExpression<TEntity>[] orderByExpressions,
            int pageNumber,
            int pageSize,
            params string[] navigationProperties
        )
        {
            IOrderedQueryable<TEntity> retVal = null;
            var result = GetQuery(filter, navigationProperties);
            var isFirstExpression = true;
            foreach (var orderExpression in orderByExpressions)
            {
                if (isFirstExpression)
                {
                    isFirstExpression = false;
                    retVal = orderExpression.IsDescending ? result.OrderByDescending(orderExpression.Expression) : result.OrderBy(orderExpression.Expression);
                }
                else
                {
                    retVal = orderExpression.IsDescending ? retVal.ThenByDescending(orderExpression.Expression) : retVal.ThenBy(orderExpression.Expression);
                }
            }
            return retVal.Skip(pageSize * (pageNumber - 1)).Take(pageSize);
        }

        public IQueryable<TEntity> GetQuery
        (
            Expression<Func<TEntity, bool>> filter,
            OrderByExpression<TEntity>[] orderByExpressions,
            params string[] navigationProperties
        )
        {
            IOrderedQueryable<TEntity> retVal = null;
            var result = GetQuery(filter, navigationProperties);
            var isFirstExpression = true;
            foreach (var orderExpression in orderByExpressions)
            {
                if (isFirstExpression)
                {
                    isFirstExpression = false;
                    retVal = orderExpression.IsDescending ? result.OrderByDescending(orderExpression.Expression) : result.OrderBy(orderExpression.Expression);
                }
                else
                {
                    retVal = orderExpression.IsDescending ? retVal.ThenByDescending(orderExpression.Expression) : retVal.ThenBy(orderExpression.Expression);
                }
            }
            return retVal;
        }

        protected string GetByKeyFilter(TKey key)
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

        protected string SetPropertyFilter(string name, object value)
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

        protected IQueryable<TEntity> GetQuery(string filter, params string[] navigationProperties) => GetAll(navigationProperties).Where(filter);

        protected IQueryable<TEntity> GetQuery(string filter, int pageNumber, int itemsPerPage, params string[] navigationProperties)
            => GetQuery(filter, navigationProperties).Skip(itemsPerPage * (pageNumber - 1)).Take(itemsPerPage);

        protected IQueryable<TEntity> GetQueryableWithNavigationProperties(params string[] navigationProperties)
        {
            IQueryable<TEntity> query = _dbContext.Set<TEntity>();
            foreach (string include in navigationProperties)
            {
                query = query.Include(include);
            }
            return query;
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
                    _dbContext.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~SqlQueryRepository()
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
