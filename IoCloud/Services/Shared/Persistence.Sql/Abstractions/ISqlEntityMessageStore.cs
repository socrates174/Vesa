using IoCloud.Shared.Messages;

namespace IoCloud.Shared.Persistence.Sql.Abstractions
{
    public interface ISqlEntityMessageStore<TEntity, TInboxMessage> : IDisposable
       where TEntity : class
       where TInboxMessage : Message
    {
        Task SaveAsync(TEntity entity, TInboxMessage inboxMessage, string requestedBy = null, CancellationToken cancellationToken = default(CancellationToken), params string[] navigationProperties);
        Task DeleteAsync(TEntity entity, TInboxMessage inboxMessage, string requestedBy = null, CancellationToken cancellationToken = default(CancellationToken));
    }

    public interface ISqlEntityMessageStore<TEntity, TInboxMessage, TOutboxMessage> : IDisposable
       where TEntity : class
       where TInboxMessage : Message
       where TOutboxMessage : Message
    {
        Task SaveAsync(TEntity entity, TInboxMessage inboxMessage, IEnumerable<TOutboxMessage> events, string requestedBy = null, CancellationToken cancellationToken = default(CancellationToken), params string[] navigationProperties);
        Task DeleteAsync(TEntity entity, TInboxMessage inboxMessage, IEnumerable<TOutboxMessage> events, string requestedBy = null, CancellationToken cancellationToken = default(CancellationToken));
    }
}
