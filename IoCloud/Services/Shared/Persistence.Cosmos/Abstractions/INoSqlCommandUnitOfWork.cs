using IoCloud.Shared.Entity.Abstractions;

namespace IoCloud.Shared.Persistence.NoSql.Abstractions
{
    public interface INoSqlCommandUnitOfWork<TRootEntity>
        where TRootEntity : class
    {
        void Add<TEntity>(TEntity entity) where TEntity : class, IEntity, IPartitionKey;
        void Update<TEntity>(TEntity entity) where TEntity : class, IEntity, IOptimisticConcurrency, IPartitionKey;
        void AddOrUpdate<TEntity>(TEntity entity) where TEntity : class, IEntity, IOptimisticConcurrency, IPartitionKey;
        void Delete<TEntity>(TEntity entity, bool forceHardDelete = false) where TEntity : class, IEntity, IOptimisticConcurrency, IPartitionKey;
        Task DeleteAsync<TEntity>(Guid id, string partitionKeyValue, bool forceHardDelete = false) where TEntity : class, IEntity, IOptimisticConcurrency, IPartitionKey;
        void Add<TEntity, TKey>(TEntity entity) where TEntity : class, IEntity<TKey>, IPartitionKey;
        void Update<TEntity, TKey>(TEntity entity) where TEntity : class, IEntity<TKey>, IOptimisticConcurrency, IPartitionKey;
        void AddOrUpdate<TEntity, TKey>(TEntity entity) where TEntity : class, IEntity<TKey>, IOptimisticConcurrency, IPartitionKey;
        void Delete<TEntity, TKey>(TEntity entity, bool forceHardDelete = false) where TEntity : class, IEntity<TKey>, IOptimisticConcurrency, IPartitionKey;
        Task DeleteAsync<TEntity, TKey>(TKey id, string partitionKeyValue, bool forceHardDelete = false) where TEntity : class, IEntity<TKey>, IOptimisticConcurrency, IPartitionKey;
        Task<IReadOnlyList<IBaseEntity>> CommitAsync(string requestedBy, CancellationToken cancellationToken = default(CancellationToken));
    }
}
