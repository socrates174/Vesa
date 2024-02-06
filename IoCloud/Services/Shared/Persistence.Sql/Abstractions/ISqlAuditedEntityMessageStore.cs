using IoCloud.Shared.Messages;

namespace IoCloud.Shared.Persistence.Sql.Abstractions
{
    public interface ISqlAuditedEntityMessageStore<TEntity, TAuditEntity, TInboxMessage> : IDisposable
       where TEntity : class
       where TAuditEntity : class
       where TInboxMessage : Message
    {
        Task SaveAsync(TEntity entity, TInboxMessage inboxMessage, string requestedBy = null, CancellationToken cancellationToken = default(CancellationToken), params string[] navigationProperties);
        Task DeleteAsync(TEntity entity, TInboxMessage inboxMessage, string requestedBy = null, CancellationToken cancellationTokenn = default(CancellationToken));
    }

    public interface ISqlAuditedEntityMessageStore<TEntity, TAuditEntity, TInboxMessage, TOutboxMessage> : IDisposable
       where TEntity : class
       where TAuditEntity : class
       where TInboxMessage : Message
       where TOutboxMessage : Message
    {
        Task SaveAsync(TEntity entity, TInboxMessage inboxMessage, IEnumerable<TOutboxMessage> outboxMessages, string requestedBy = null, CancellationToken cancellationToken = default(CancellationToken), params string[] navigationProperties);
        Task DeleteAsync(TEntity entity, TInboxMessage inboxMessage, IEnumerable<TOutboxMessage> outboxMessages, string requestedBy = null, CancellationToken cancellationToken = default(CancellationToken));
    }
}
