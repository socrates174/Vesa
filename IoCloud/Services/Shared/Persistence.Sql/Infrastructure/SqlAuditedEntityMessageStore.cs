using AutoMapper;
using IoCloud.Shared.Messages;
using IoCloud.Shared.Persistence.Sql.Abstractions;

namespace IoCloud.Shared.Persistence.Sql.Infrastructure
{
    public class SqlAuditedEntityMessageStore<TEntity, TAuditEntity, TInboxMessage> : ISqlAuditedEntityMessageStore<TEntity, TAuditEntity, TInboxMessage>
        where TEntity : class
        where TAuditEntity : class
        where TInboxMessage : Message
    {
        protected readonly ISqlUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private bool disposedValue;

        public SqlAuditedEntityMessageStore(ISqlUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public virtual async Task SaveAsync
        (
            TEntity entity,
            TInboxMessage inboxMessage,
            string requestedBy = null, 
            CancellationToken cancellationToken = default(CancellationToken),
            params string[] navigationProperties
        )
        {
            var entityRepository = _unitOfWork.CreateRepository<TEntity>();
            var auditEntityRepository = _unitOfWork.CreateRepository<TAuditEntity>();

            await entityRepository.AddOrUpdateAsync(entity, navigationProperties);
            var auditEntity = _mapper.Map<TEntity, TAuditEntity>(entity);
            auditEntityRepository.Add(auditEntity);
            AddMessagesToUnitOfWork(inboxMessage);

            await _unitOfWork.CommitAsync(requestedBy, cancellationToken);
        }

        public virtual async Task DeleteAsync
        (
            TEntity entity,
            TInboxMessage inboxMessage,
            string requestedBy = null, 
            CancellationToken cancellationToken = default(CancellationToken)
        )
        {
            var entityRepository = _unitOfWork.CreateRepository<TEntity>();
            var auditEntityRepository = _unitOfWork.CreateRepository<TAuditEntity>();

            await entityRepository.DeleteAsync(entity);
            var auditEntity = _mapper.Map<TEntity, TAuditEntity>(entity);
            auditEntityRepository.Add(auditEntity);
            AddMessagesToUnitOfWork(inboxMessage);

            await _unitOfWork.CommitAsync(requestedBy, cancellationToken);
        }

        protected void AddMessagesToUnitOfWork(TInboxMessage inboxMessage)
        {
            var inboxMessageRepository = _unitOfWork.CreateRepository<TInboxMessage>();

            if (inboxMessage != null)
            {
                if (!string.IsNullOrWhiteSpace(inboxMessage.Subject))
                {
                    inboxMessageRepository.Add(inboxMessage);
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
                    _unitOfWork.Dispose();
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

    public class SqlAuditedEntityMessageStore<TEntity, TAuditEntity, TInboxMessage, TOutboxMessage> : ISqlAuditedEntityMessageStore<TEntity, TAuditEntity, TInboxMessage, TOutboxMessage>
        where TEntity : class
        where TAuditEntity : class
        where TInboxMessage : Message
        where TOutboxMessage : Message
    {
        protected readonly ISqlUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private bool disposedValue;

        public SqlAuditedEntityMessageStore(ISqlUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public virtual async Task SaveAsync
        (
            TEntity entity,
            TInboxMessage inboxMessage,
            IEnumerable<TOutboxMessage> outboxMessages,
            string requestedBy = null,
            CancellationToken cancellationToken = default(CancellationToken),
            params string[] navigationProperties
        )
        {
            var entityRepository = _unitOfWork.CreateRepository<TEntity>();
            var auditEntityRepository = _unitOfWork.CreateRepository<TAuditEntity>();

            await entityRepository.AddOrUpdateAsync(entity, navigationProperties);
            var auditEntity = _mapper.Map<TEntity, TAuditEntity>(entity);
            auditEntityRepository.Add(auditEntity);
            AddMessagesToUnitOfWork(inboxMessage, outboxMessages);

            await _unitOfWork.CommitAsync(requestedBy, cancellationToken);
        }

        public virtual async Task DeleteAsync
        (
            TEntity entity,
            TInboxMessage inboxMessage,
            IEnumerable<TOutboxMessage> outboxMessages,
            string requestedBy = null,
            CancellationToken cancellationToken = default(CancellationToken)
        )
        {
            var entityRepository = _unitOfWork.CreateRepository<TEntity>();
            var auditEntityRepository = _unitOfWork.CreateRepository<TAuditEntity>();

            await entityRepository.DeleteAsync(entity);
            var auditEntity = _mapper.Map<TEntity, TAuditEntity>(entity);
            auditEntityRepository.Add(auditEntity);
            AddMessagesToUnitOfWork(inboxMessage, outboxMessages);

            await _unitOfWork.CommitAsync(requestedBy, cancellationToken);
        }

        protected void AddMessagesToUnitOfWork(TInboxMessage inboxMessage, IEnumerable<TOutboxMessage> outboxMessages)
        {
            var inboxMessageRepository = _unitOfWork.CreateRepository<TInboxMessage>();
            var outboxMessageRepository = _unitOfWork.CreateRepository<TOutboxMessage>();

            if (inboxMessage != null)
            {
                if (!string.IsNullOrWhiteSpace(inboxMessage.Subject))
                {
                    inboxMessageRepository.Add(inboxMessage);
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
                        outboxMessageRepository.Add(outboxMessage);
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
                    _unitOfWork.Dispose();
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
}
