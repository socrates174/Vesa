using IoCloud.Shared.Entity.Abstractions;

namespace IoCloud.Shared.Querying.NoSql.Abstractions
{
    public interface INoSqlQueryRepository<TEntity> : INoSqlQueryRepository<TEntity, Guid>
        where TEntity : class, IEntity
    {
    }

    public interface INoSqlQueryRepository<TEntity, TKey> : IDisposable
        where TEntity : class, IEntity<TKey>
    {
        Task<TEntity> GetAsync(TKey id, bool includeSoftDeleted = false);
        Task<TEntity> GetAsync(TKey id, string partitionKeyValue, bool includeSoftDeleted = false);
        Task<IReadOnlyList<TEntity>> GetListAsync(string sqlQueryText, bool includeSoftDeleted = false, params (string Name, object Value)[] parameters);
        IQueryable<TEntity> GetQuery();
    }
}
