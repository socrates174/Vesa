using IoCloud.Shared.Entity.Abstractions;
using IoCloud.Shared.Entity.Infrastructure;
using IoCloud.Shared.Messages;
using IoCloud.Shared.Persistence.NoSql.Abstractions;
using IoCloud.Shared.Querying.NoSql.Abstractions;

namespace IoCloud.Shared.Persistence.NoSql.Infrastructure
{
    /// <summary>
    /// Saves / deletes an entity, saves the audit entity, the inbox message (command or event)
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TInboxMessage"></typeparam>
    public class CosmosEntityMessageStore<TEntity, TKey, TInboxMessage> : INoSqlEntityMessageStore<TEntity, TKey, TInboxMessage>
        where TEntity : class, IEntity<TKey>, IOptimisticConcurrency, IPartitionKey
        where TInboxMessage : Message, IPartitionKey
    {
        protected bool disposedValue;
        protected readonly INoSqlCommandUnitOfWork<TEntity> _noSqlCommandUnitOfWork;

        public CosmosEntityMessageStore(INoSqlCommandUnitOfWork<TEntity> noSqlCommandUnitOfWork)
        {
            _noSqlCommandUnitOfWork = noSqlCommandUnitOfWork;
        }

        public virtual async Task<TEntity> SaveAsync
        (
            TEntity entity, 
            TInboxMessage inboxMessage, 
            string requestedBy, 
            CancellationToken cancellationToken = default(CancellationToken)
        )
        {
            Save(entity, inboxMessage);
            var entities = await _noSqlCommandUnitOfWork.CommitAsync(requestedBy, cancellationToken);
            return (TEntity)entities.First();
        }

        public virtual async Task DeleteAsync
        (
            TEntity entity, 
            TInboxMessage inboxMessage, 
            string requestedBy, 
            CancellationToken cancellationToken = default(CancellationToken)
        )
        {
            Delete(entity, inboxMessage);
            await _noSqlCommandUnitOfWork.CommitAsync(requestedBy, cancellationToken);
        }

        public virtual async Task DeleteAsync(TKey id, string partitionKeyValue, TInboxMessage inboxMessage, string requestedBy, CancellationToken cancellationToken = default(CancellationToken))
        {
            await DeleteAsync(id, partitionKeyValue, inboxMessage);
            await _noSqlCommandUnitOfWork.CommitAsync(requestedBy, cancellationToken);
        }

        protected void Save(TEntity entity, TInboxMessage inboxMessage)
        {
            _noSqlCommandUnitOfWork.AddOrUpdate<TEntity, TKey>(entity);
            AddMessagesToUnitOfWork(inboxMessage);
        }

        protected void Delete(TEntity entity, TInboxMessage inboxMessage)
        {
            _noSqlCommandUnitOfWork.Delete<TEntity, TKey>(entity);
            AddMessagesToUnitOfWork(inboxMessage);
        }

        protected async Task DeleteAsync(TKey id, string partitionKeyValue, TInboxMessage inboxMessage)
        {
            await _noSqlCommandUnitOfWork.DeleteAsync<TEntity, TKey>(id, partitionKeyValue);
            AddMessagesToUnitOfWork(inboxMessage);
        }
        
        protected void AddMessagesToUnitOfWork(TInboxMessage inboxMessage)
        {
            if (inboxMessage != null)
            {
                if (!string.IsNullOrWhiteSpace(inboxMessage.Subject))
                {
                    _noSqlCommandUnitOfWork.Add<TInboxMessage, string>(inboxMessage);
                }
                else
                {
                    throw new Exception($"Missing {inboxMessage.EntityType}.Subject for Id:{inboxMessage.Id}");
                }
            }
        }

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
        // ~EntityMessageStore()
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

    /// <summary>
    /// Saves / deletes an entity, saves the inbox message (command or event), outbox message(s) (domain event(s))
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TInboxMessage"></typeparam>
    /// <typeparam name="TOutboxMessage"></typeparam>
    public class CosmosEntityMessageStore<TEntity, TKey, TInboxMessage, TOutboxMessage> : INoSqlEntityMessageStore<TEntity, TKey, TInboxMessage, TOutboxMessage>
        where TEntity : class, IEntity<TKey>, IOptimisticConcurrency, IPartitionKey
        where TInboxMessage : Message, IPartitionKey
        where TOutboxMessage : Message, IPartitionKey
    {
        protected bool disposedValue;
        protected readonly INoSqlCommandUnitOfWork<TEntity> _noSqlCommandUnitOfWork;

        public CosmosEntityMessageStore(INoSqlCommandUnitOfWork<TEntity> noSqlCommandUnitOfWork)
        {
            _noSqlCommandUnitOfWork = noSqlCommandUnitOfWork;
        }

        public virtual async Task<TEntity> SaveAsync
        (
            TEntity entity, 
            TInboxMessage inboxMessage, 
            IEnumerable<TOutboxMessage> outboxMessages, 
            string requestedBy, 
            CancellationToken cancellationToken = default(CancellationToken)
        )
        {
            Save(entity, inboxMessage, outboxMessages);
            var entities = await _noSqlCommandUnitOfWork.CommitAsync(requestedBy, cancellationToken);
            return (TEntity)entities.First();
        }

        public virtual async Task DeleteAsync
        (
            TEntity entity, 
            TInboxMessage inboxMessage, 
            IEnumerable<TOutboxMessage> outboxMessages, 
            string requestedBy, 
            CancellationToken cancellationToken = default(CancellationToken)
        )
        {
            Delete(entity, inboxMessage, outboxMessages);
            await _noSqlCommandUnitOfWork.CommitAsync(requestedBy, cancellationToken);
        }

        public virtual async Task DeleteAsync
        (
            TKey id, 
            string partitionKeyValue, 
            TInboxMessage inboxMessage, 
            IEnumerable<TOutboxMessage> outboxMessages, 
            string requestedBy, 
            CancellationToken cancellationToken = default(CancellationToken)
        )
        {
            await DeleteAsync(id, partitionKeyValue, inboxMessage, outboxMessages);
            await _noSqlCommandUnitOfWork.CommitAsync(requestedBy, cancellationToken);
        }

        protected void Save(TEntity entity, TInboxMessage inboxMessage, IEnumerable<TOutboxMessage> outboxMessages)
        {
            _noSqlCommandUnitOfWork.AddOrUpdate<TEntity, TKey>(entity);
            AddMessagesToUnitOfWork(inboxMessage, outboxMessages);
        }

        protected void Delete(TEntity entity, TInboxMessage inboxMessage, IEnumerable<TOutboxMessage> outboxMessages)
        {
            _noSqlCommandUnitOfWork.Delete<TEntity, TKey>(entity);
            AddMessagesToUnitOfWork(inboxMessage, outboxMessages);
        }

        protected async Task DeleteAsync(TKey id, string partitionKeyValue, TInboxMessage inboxMessage, IEnumerable<TOutboxMessage> outboxMessages)
        {
            await _noSqlCommandUnitOfWork.DeleteAsync<TEntity, TKey>(id, partitionKeyValue);
            AddMessagesToUnitOfWork(inboxMessage, outboxMessages);
        }

        protected void AddMessagesToUnitOfWork(TInboxMessage inboxMessage, IEnumerable<TOutboxMessage> outboxMessages)
        {
            if (inboxMessage != null)
            {
                if (!string.IsNullOrWhiteSpace(inboxMessage.Subject))
                {
                    _noSqlCommandUnitOfWork.Add<TInboxMessage, string>(inboxMessage);
                }
                else
                {
                    throw new Exception($"Missing {inboxMessage.EntityType}.Subject for Id:{inboxMessage.Id}");
                }
            }
            if (outboxMessages != null)
            {
                foreach (var outboxMessage in outboxMessages)
                {
                    if (!string.IsNullOrWhiteSpace(outboxMessage.Subject))
                    {
                        _noSqlCommandUnitOfWork.Add<TOutboxMessage, string>(outboxMessage);
                    }
                    else
                    {
                        throw new Exception($"Missing {outboxMessage.EntityType}.Subject for Id:{outboxMessage.Id}");
                    }
                }
            }
        }

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
        // ~EntityMessageStore()
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

    /// <summary>
    /// Saves / deletes an entity, saves the audit entity, the inbox message (command or event)
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TAuditEntity"></typeparam>
    /// <typeparam name="TAuditKey"></typeparam>
    /// <typeparam name="TInboxMessage"></typeparam>
    public class CosmosEntityMessageStore<TEntity, TKey, TAuditEntity, TAuditKey, TInboxMessage> : CosmosEntityMessageStore<TEntity, TKey, TInboxMessage>,
                                                                                                     INoSqlEntityMessageStore<TEntity, TKey, TAuditEntity, TAuditKey, TInboxMessage>
        where TEntity : class, IEntity<TKey>, IOptimisticConcurrency, IPartitionKey
        where TAuditEntity : NoSqlAudit<TAuditKey, TKey>, new()
        where TInboxMessage : Message, IPartitionKey
    {
        private readonly INoSqlQueryRepository<TEntity, TKey> _noSqlQueryRepository;

        public CosmosEntityMessageStore
        (
            INoSqlCommandUnitOfWork<TEntity> noSqlCommandUnitOfWork, 
            INoSqlQueryRepository<TEntity, TKey> noSqlQueryRepository
        ) 
            : base(noSqlCommandUnitOfWork)
        {
            _noSqlQueryRepository = noSqlQueryRepository;
        }

        public override async Task<TEntity> SaveAsync
        (
            TEntity entity, 
            TInboxMessage inboxMessage, 
            string requestedBy, 
            CancellationToken cancellationToken = default(CancellationToken)
        )
        {
            Save(entity, inboxMessage);
            var auditEntity = new TAuditEntity();
            auditEntity.SetAuditedEntity(entity);
            _noSqlCommandUnitOfWork.Add<TAuditEntity, TAuditKey>(auditEntity);
            var entities = await _noSqlCommandUnitOfWork.CommitAsync(requestedBy, cancellationToken);
            return (TEntity)entities.First();
        }

        public override async Task DeleteAsync
        (
            TEntity entity, 
            TInboxMessage inboxMessage, 
            string requestedBy, 
            CancellationToken cancellationToken = default(CancellationToken)
        )
        {
            Delete(entity, inboxMessage);
            var auditEntity = new TAuditEntity();
            auditEntity.SetAuditedEntity(entity, true);
            _noSqlCommandUnitOfWork.Add<TAuditEntity, TAuditKey>(auditEntity);
            var entities = await _noSqlCommandUnitOfWork.CommitAsync(requestedBy, cancellationToken);
        }

        public virtual async Task DeleteAsync
        (
            TKey id, 
            string partitionKeyValue, 
            TInboxMessage inboxMessage, 
            string requestedBy, 
            CancellationToken cancellationToken = default(CancellationToken)
        )
        {
            var entity = await _noSqlQueryRepository.GetAsync(id, partitionKeyValue);
            await DeleteAsync(entity, inboxMessage, requestedBy, cancellationToken);
        }
    }

    /// <summary>
    /// Saves / deletes an entity, saves the audit entity, the inbox message (command or event), outbox message(s) (domain event(s))
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TAuditEntity"></typeparam>
    /// <typeparam name="TAuditKey"></typeparam>
    /// <typeparam name="TInboxMessage"></typeparam>
    /// <typeparam name="TOutboxMessage"></typeparam>
    public class CosmosEntityMessageStore<TEntity, TKey, TAuditEntity, TAuditKey, TInboxMessage, TOutboxMessage> : CosmosEntityMessageStore<TEntity, TKey, TInboxMessage, TOutboxMessage>,
                                                                                                     INoSqlEntityMessageStore<TEntity, TKey, TAuditEntity, TAuditKey, TInboxMessage, TOutboxMessage>
        where TEntity : class, IEntity<TKey>, IOptimisticConcurrency, IPartitionKey
        where TAuditEntity : NoSqlAudit<TAuditKey, TKey>, new()
        where TInboxMessage : Message, IPartitionKey
        where TOutboxMessage : Message, IPartitionKey
    {
        private readonly INoSqlQueryRepository<TEntity, TKey> _noSqlQueryRepository;

        public CosmosEntityMessageStore
        (
            INoSqlCommandUnitOfWork<TEntity> noSqlCommandUnitOfWork,
            INoSqlQueryRepository<TEntity, TKey> noSqlQueryRepository
        )
            : base(noSqlCommandUnitOfWork)
        {
            _noSqlQueryRepository = noSqlQueryRepository;
        }

        public override async Task<TEntity> SaveAsync
        (
            TEntity entity, 
            TInboxMessage inboxMessage, 
            IEnumerable<TOutboxMessage> outboxMessages, 
            string requestedBy, 
            CancellationToken cancellationToken = default(CancellationToken))
        {
            Save(entity, inboxMessage, outboxMessages);
            var auditEntity = new TAuditEntity();
            auditEntity.SetAuditedEntity(entity);
            _noSqlCommandUnitOfWork.Add<TAuditEntity, TAuditKey>(auditEntity);
            var entities = await _noSqlCommandUnitOfWork.CommitAsync(requestedBy, cancellationToken);
            return (TEntity)entities.First();
        }

        public override async Task DeleteAsync
        (
            TEntity entity, 
            TInboxMessage inboxMessage, 
            IEnumerable<TOutboxMessage> outboxMessages, 
            string requestedBy, 
            CancellationToken cancellationToken = default(CancellationToken)
        )
        {
            Delete(entity, inboxMessage, outboxMessages);
            var auditEntity = new TAuditEntity();
            auditEntity.SetAuditedEntity(entity, true);
            _noSqlCommandUnitOfWork.Add<TAuditEntity, TAuditKey>(auditEntity);
            var entities = await _noSqlCommandUnitOfWork.CommitAsync(requestedBy, cancellationToken);
        }

        public virtual async Task DeleteAsync
        (
            TKey id, 
            string partitionKeyValue, 
            TInboxMessage inboxMessage, 
            IEnumerable<TOutboxMessage> outboxMessages, 
            string requestedBy, 
            CancellationToken cancellationToken = default(CancellationToken)
        )
        {
            var entity = await _noSqlQueryRepository.GetAsync(id, partitionKeyValue);
            await DeleteAsync(entity, inboxMessage, outboxMessages, requestedBy, cancellationToken);
        }
    }
}
