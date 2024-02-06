using System.Linq.Expressions;

namespace IoCloud.Shared.Querying.Sql.Abstractions
{
    public interface ISqlQueryRepository<TEntity> : ISqlQueryRepository<TEntity, Guid>
        where TEntity : class
    {
    }

    public interface ISqlQueryRepository<TEntity, TKey> : IDisposable
      where TEntity : class
    {
        object GetKeyValue(TEntity entity);
        Task<TEntity> GetAsync(TKey key, params string[] navigationProperties);
        IQueryable<TEntity> GetAll(params string[] includes);
        IQueryable<TEntity> GetQuery(Expression<Func<TEntity, bool>> filter, params string[] includes);
        IQueryable<TEntity> GetQuery(Expression<Func<TEntity, bool>> filter, int pageNumber, int pageSize, params string[] includes);
        IQueryable<TEntity> GetQuery(Expression<Func<TEntity, bool>> filter, OrderByExpression<TEntity>[] orderByExpressions, params string[] includes);
        IQueryable<TEntity> GetQuery(Expression<Func<TEntity, bool>> filter, OrderByExpression<TEntity>[] orderByExpressions, int pageNumber, int pageSize, params string[] includes);
    }
}