using IoCloud.Shared.Messages;
using System.Linq.Expressions;

namespace IoCloud.Shared.Persistence.NoSql.Abstractions
{
    public interface ICloudEventStore<TEvent> : IDisposable
        where TEvent : CloudEventMessage
    {
        Task AddAsync(TEvent theEvent, string requestedBy = null, CancellationToken cancellationToken = default(CancellationToken));
        Task AddAsync(IEnumerable<TEvent> events, string requestedBy = null, CancellationToken cancellationToken = default(CancellationToken));
        Task<TEntity> ReplayAsync<TEntity>(Guid id, DateTimeOffset toDate);
        Task<TEntity> ReplayAsync<TEntity>(Guid id, Expression<Func<TEvent, bool>> filter = null);
        Task<TEntity> ReplayAsync<TEntity, TKey>(TKey id, DateTimeOffset toDate);
        Task<TEntity> ReplayAsync<TEntity, TKey>(TKey id, Expression<Func<TEvent, bool>> filter = null);
        Task<TEntity> ReplayStreamAsync<TEntity>(string eventStreamKey, DateTimeOffset toDate);
        Task<TEntity> ReplayStreamAsync<TEntity>(string eventStreamKey, Expression<Func<TEvent, bool>> filter = null);

        /// Use these methods if you have different entity versions
        Task<object> ReplayVersionedStreamAsync<TEntityV1>(string eventStreamKey, DateTimeOffset toDate);
        Task<object> ReplayVersionedStreamAsync<TEntityV1>(string eventStreamKey, Expression<Func<TEvent, bool>> filter = null);
    }

    public interface ICloudEventStore<TInboxMessage, TOutboxEvent> : IDisposable
        where TInboxMessage : CloudEventMessage
        where TOutboxEvent : CloudEventMessage
    {
        Task AddAsync(TInboxMessage inboxMessage, TOutboxEvent outboxEvent, string requestedBy = null, CancellationToken cancellationToken = default(CancellationToken));
        Task AddAsync(TInboxMessage inboxMessage, IEnumerable<TOutboxEvent> outboxEvents, string requestedBy = null, CancellationToken cancellationToken = default(CancellationToken));
        Task<TEntity> ReplayAsync<TEntity>(Guid id, DateTimeOffset toDate);
        Task<TEntity> ReplayAsync<TEntity>(Guid id, Expression<Func<TOutboxEvent, bool>> filter = null);
        Task<TEntity> ReplayAsync<TEntity, TKey>(TKey id, DateTimeOffset toDate);
        Task<TEntity> ReplayAsync<TEntity, TKey>(TKey id, Expression<Func<TOutboxEvent, bool>> filter = null);
        Task<TEntity> ReplayStreamAsync<TEntity>(string eventStreamKey, DateTimeOffset toDate);
        Task<TEntity> ReplayStreamAsync<TEntity>(string eventStreamKey, Expression<Func<TOutboxEvent, bool>> filter = null);

        /// Use these methods if you have different entity versions
        Task<object> ReplayVersionedStreamAsync<TEntityV1>(string eventStreamKey, DateTimeOffset toDate);
        Task<object> ReplayVersionedStreamAsync<TEntityV1>(string eventStreamKey, Expression<Func<TOutboxEvent, bool>> filter = null);
    }
}