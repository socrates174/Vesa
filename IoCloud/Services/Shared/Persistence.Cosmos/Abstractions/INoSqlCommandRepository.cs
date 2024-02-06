using IoCloud.Shared.Entity.Abstractions;

namespace IoCloud.Shared.Persistence.NoSql.Abstractions
{
    public interface INoSqlCommandRepository<TEntity> : INoSqlCommandRepository<TEntity, Guid>
        where TEntity : class, IEntity, IOptimisticConcurrency, IPartitionKey
    {
    }

    public interface INoSqlCommandRepository<TEntity, TKey> : IDisposable
        where TEntity : class, IEntity<TKey>, IOptimisticConcurrency, IPartitionKey
    {
        Task<TEntity> AddAsync(TEntity entity, string requestedBy = null, CancellationToken cancellationToken = default(CancellationToken));
        Task<TEntity> UpdateAsync(TEntity entity, string requestedBy = null, CancellationToken cancellationToken = default(CancellationToken));
        Task<TEntity> AddOrUpdateAsync(TEntity entity, string requestedBy = null, CancellationToken cancellationToken = default(CancellationToken));
        Task DeleteAsync(TEntity entity, bool forceHardDelete = false, string requestedBy = null, CancellationToken cancellationToken = default(CancellationToken));
        Task DeleteAsync(TKey id, string partitionKeyValue, bool forceHardDelete = false, string requestedBy = null, CancellationToken cancellationToken = default(CancellationToken));
    }

    public interface INoSqlCommandRepository<TEntity, TKey, TAuditEntity, TAuditKey> : INoSqlCommandRepository<TEntity, TKey>
        where TEntity : class, IEntity<TKey>, IOptimisticConcurrency, IPartitionKey
        where TAuditEntity : class, IEntity<TAuditKey>, ISoftDelete, IPartitionKey, new()
    {
    }
}
