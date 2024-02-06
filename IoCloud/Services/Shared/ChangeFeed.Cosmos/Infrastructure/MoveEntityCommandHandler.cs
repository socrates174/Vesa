using AutoMapper;
using IoCloud.Shared.Entity.Abstractions;
using IoCloud.Shared.MessageHandling.Infrastructure;
using IoCloud.Shared.Messages;
using IoCloud.Shared.Persistence.NoSql.Abstractions;
using IoCloud.Shared.Utility;
using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace IoCloud.Shared.ChangeFeed.Infrastructure
{
    // ***TODO: using IRequestHandler is a temporary fix until I figure out why ICommandHandler does not work
    /// <summary>
    /// Moves an entity detected by the change feed from the source container to a target container
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    public class MoveEntityCommandHandler<TEntity, TKey, TRootEntity> : IRequestHandler<ChangeFeedCommand<TEntity, TRootEntity>, VoidReply>, IDisposable
        //ICommandHandler<ChangeFeedCommand<TEntity>, VoidReply>  
        where TEntity : class, IEntity<TKey>, IOptimisticConcurrency, IPartitionKey
    {
        const string SYSTEM_USER = "System";

        private INoSqlCommandRepository<TEntity, TKey> _sourceCommandRepository;
        private INoSqlCommandRepository<TEntity, TKey> _targetCommandRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<MoveEntityCommandHandler<TEntity, TKey, TRootEntity>> _logger;
        private bool disposedValue;

        public MoveEntityCommandHandler
        (
            INoSqlCommandRepository<TEntity, TKey> sourceCommandRepository,
            INoSqlCommandRepository<TEntity, TKey> targetCommandRepository,
            IMapper mapper,
            ILogger<MoveEntityCommandHandler<TEntity, TKey, TRootEntity>> logger
        )
        {
            _sourceCommandRepository = sourceCommandRepository;
            _targetCommandRepository = targetCommandRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<VoidReply> Handle(ChangeFeedCommand<TEntity, TRootEntity> command, CancellationToken cancellationToken)
        {
            TEntity sourceEntity = default(TEntity);
            try
            {
                sourceEntity = command.Data;
                var targetEntity = sourceEntity.Clone();
                if (targetEntity is CloudEventMessage cloudEvent)
                {
                    if (!string.IsNullOrWhiteSpace(cloudEvent.Subject))
                    {
                        targetEntity.PartitionKey = cloudEvent.Subject;
                    }
                    else
                    {
                        throw new Exception($"Missing {targetEntity.EntityType}.Subject for Id:{targetEntity.Id}");
                    }
                }
                else if (targetEntity is INoSqlAudit noSqlAudit)
                {
                    targetEntity.PartitionKey = $"{GetTypeShortName(noSqlAudit.AuditedEntityType)}/{targetEntity.PartitionKey}";
                }
                targetEntity.ConcurrencyToken = null;

                await _targetCommandRepository.AddAsync(targetEntity, SYSTEM_USER, cancellationToken);
                await _sourceCommandRepository.DeleteAsync(sourceEntity, true, SYSTEM_USER, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex.Message}: Message: {JsonConvert.SerializeObject(sourceEntity)}");
                throw;
            }
            return new VoidReply();
        }

        private string GetTypeShortName(string typeFullName)
        {
            string shortName = typeFullName;
            var index = typeFullName.LastIndexOf(".");
            if (index > -1)
            {
                shortName = typeFullName.Substring(index + 1);
            }
            return shortName;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _sourceCommandRepository.Dispose();
                    _targetCommandRepository.Dispose();
                    _sourceCommandRepository = null;
                    _targetCommandRepository = null;
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~MoveEntityCommandHandler()
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
