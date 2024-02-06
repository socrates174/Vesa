using IoCloud.Shared.Entity.Abstractions;
using IoCloud.Shared.Messages;

namespace IoCloud.Shared.Persistence.NoSql.Abstractions
{
    public interface INoSqlEntityMessageStore<TEntity, TKey, TInboxMessage> : IDisposable
        where TEntity : class, IEntity<TKey>, IOptimisticConcurrency, IPartitionKey
        where TInboxMessage : Message, IPartitionKey
    {
        Task<TEntity> SaveAsync(TEntity entity, TInboxMessage inboxMessage, string requestedBy, CancellationToken cancellationToken = default(CancellationToken));
        Task DeleteAsync(TEntity entity, TInboxMessage inboxMessage, string requestedBy, CancellationToken cancellationToken = default(CancellationToken));
        Task DeleteAsync(TKey id, string partitionKeyValue, TInboxMessage inboxMessage, string requestedBy, CancellationToken cancellationToken = default(CancellationToken));
    }

    public interface INoSqlEntityMessageStore<TEntity, TKey, TAuditEntity, TAuditKey, TInboxMessage> : INoSqlEntityMessageStore<TEntity, TKey, TInboxMessage>
        where TEntity : class, IEntity<TKey>, IOptimisticConcurrency, IPartitionKey
        where TAuditEntity : class, IEntity<TAuditKey>, IOptimisticConcurrency, ISoftDelete, IAuditable, IPartitionKey, new()
        where TInboxMessage : Message, IPartitionKey
    {
    }

    public interface INoSqlEntityMessageStore<TEntity, TKey, TInboxMessage, TOutboxMessage> : IDisposable
        where TEntity : class, IEntity<TKey>, IOptimisticConcurrency, IPartitionKey
        where TInboxMessage : Message, IPartitionKey
        where TOutboxMessage : Message, IPartitionKey
    {
        Task<TEntity> SaveAsync(TEntity entity, TInboxMessage inboxMessage, IEnumerable<TOutboxMessage> outboxMessages, string requestedBy, CancellationToken cancellationToken = default(CancellationToken));
        Task DeleteAsync(TEntity entity, TInboxMessage inboxMessage, IEnumerable<TOutboxMessage> outboxMessages, string requestedBy, CancellationToken cancellationToken = default(CancellationToken));
        Task DeleteAsync(TKey id, string partitionKeyValue, TInboxMessage inboxMessage, IEnumerable<TOutboxMessage> outboxMessages, string requestedBy, CancellationToken cancellationToken = default(CancellationToken));
    }

    public interface INoSqlEntityMessageStore<TEntity, TKey, TAuditEntity, TAuditKey, TInboxMessage, TOutboxMessage> : INoSqlEntityMessageStore<TEntity, TKey, TInboxMessage, TOutboxMessage>
        where TEntity : class, IEntity<TKey>, IOptimisticConcurrency, IPartitionKey
        where TAuditEntity : class, IEntity<TAuditKey>, IOptimisticConcurrency, ISoftDelete, IAuditable, IPartitionKey, new()
        where TInboxMessage : Message, IPartitionKey
        where TOutboxMessage : Message, IPartitionKey

    {
    }
}
