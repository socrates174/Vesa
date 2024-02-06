using IoCloud.Shared.Entity.Abstractions;
using IoCloud.Shared.Entity.Extensions;
using IoCloud.Shared.Entity.Infrastructure;
using IoCloud.Shared.Persistence.NoSql.Abstractions;
using IoCloud.Shared.Settings.Abstractions;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace IoCloud.Shared.Persistence.NoSql.Infrastructure
{
    /// <summary>
    /// Adds, Update, AddOrUpdate, Delete an entity
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public class CosmosCommandRepository<TEntity> : CosmosCommandRepository<TEntity, Guid>, INoSqlCommandRepository<TEntity>
        where TEntity : class, IEntity, IOptimisticConcurrency, IPartitionKey
    {
        public CosmosCommandRepository
        (
            ICosmosContainerConfiguration<TEntity> configuration,
            CosmosClient client,
            ILogger<CosmosCommandRepository<TEntity>> logger
        )
            : base(configuration, client, logger)
        {
        }
    }

    public class CosmosCommandRepository<TEntity, TKey> : CosmosCommandRepository<TEntity, TKey, TEntity>
      where TEntity : class, IEntity<TKey>, IOptimisticConcurrency, IPartitionKey
    {

        public CosmosCommandRepository
        (
            ICosmosContainerConfiguration<TEntity> configuration,
            CosmosClient client,
            ILogger<CosmosCommandRepository<TEntity, TKey, TEntity>> logger
        )
            : base(configuration, client, logger)
        {
        }
    }

    public class CosmosCommandRepository<TEntity, TKey, TRootEntity> : INoSqlCommandRepository<TEntity, TKey>
        where TEntity : class, IEntity<TKey>, IOptimisticConcurrency, IPartitionKey
        where TRootEntity : class
    {

        protected bool disposedValue;

        protected readonly ICosmosContainerConfiguration<TRootEntity> _configuration;
        protected readonly CosmosClient _client;
        protected readonly ILogger<CosmosCommandRepository<TEntity, TKey, TRootEntity>> _logger;
        protected string _partitionKeyValue;

        public CosmosCommandRepository
        (
            ICosmosContainerConfiguration<TRootEntity> configuration,
            CosmosClient client,
            ILogger<CosmosCommandRepository<TEntity, TKey, TRootEntity>> logger
        )
        {
            _configuration = configuration;
            _client = client;
            _logger = logger;
        }

        public virtual async Task<TEntity> AddAsync(TEntity entity, string requestedBy = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                ItemResponse<TEntity> itemResponse;
                var container = GetContainer();
                var partitionKey = GetPartitionKey(entity);

                (entity as IAuditable).Stamp(requestedBy);

                if (partitionKey == null)
                {
                    itemResponse = await container.CreateItemAsync(entity, null, null, cancellationToken);
                }
                else
                {
                    itemResponse = await container.CreateItemAsync(entity, partitionKey, null, cancellationToken);
                }
                _partitionKeyValue = null;  // reset

                return itemResponse.Resource;
            }
            catch (CosmosException ex)
            {
                _logger.LogError($"New entity with ID: {entity.Id} was not added successfully - error details: {ex.Message}");
                throw;
            }
        }

        public virtual async Task<TEntity> UpdateAsync(TEntity entity, string requestedBy = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                var container = GetContainer();
                var id = entity.Id.ToString();
                var partitionKey = GetPartitionKey(entity);
                var requestOptions = new ItemRequestOptions { IfMatchEtag = entity.ConcurrencyToken };

                (entity as IAuditable).Stamp(requestedBy);

                var itemResponse = await container.ReplaceItemAsync(entity, id, partitionKey, requestOptions, cancellationToken);
                _partitionKeyValue = null;  // reset

                return itemResponse.Resource;
            }
            catch (CosmosException ex)
            {
                _logger.LogError($"Entity with ID: {entity.Id} was not updated successfully - error details: {ex.Message}");
                throw;
            }
        }

        public virtual async Task<TEntity> AddOrUpdateAsync(TEntity entity, string requestedBy = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                ItemResponse<TEntity> itemResponse;
                var container = GetContainer();
                var partitionKey = GetPartitionKey(entity);
                var requestOptions = new ItemRequestOptions { IfMatchEtag = entity.ConcurrencyToken };

                (entity as IAuditable).Stamp(requestedBy);

                if (partitionKey == null)
                {
                    itemResponse = await container.UpsertItemAsync(entity, null, requestOptions, cancellationToken);
                }
                else
                {
                    itemResponse = await container.UpsertItemAsync(entity, partitionKey, requestOptions, cancellationToken);
                }
                _partitionKeyValue = null;  // reset

                return itemResponse.Resource;
            }
            catch (CosmosException ex)
            {
                _logger.LogError($"New entity with ID: {entity.Id} was not added successfully - error details: {ex.Message}");
                throw;
            }
        }

        public virtual async Task DeleteAsync(TEntity entity, bool forceHardDelete = false, string requestedBy = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                var container = GetContainer();
                var partitionKey = GetPartitionKey(entity);
                var softDelete = entity as ISoftDelete;
                if (forceHardDelete || softDelete == null)
                {
                    var requestOptions = new ItemRequestOptions { EnableContentResponseOnWrite = false };
                    var response = await container.DeleteItemAsync<TEntity>(entity.Id.ToString(), partitionKey, requestOptions, cancellationToken);
                    _partitionKeyValue = null;  // reset
                }
                else if (softDelete != null && !softDelete.IsDeleted)
                {
                    var requestOptions = new ItemRequestOptions { IfMatchEtag = entity.ConcurrencyToken, EnableContentResponseOnWrite = false };
                    softDelete.IsDeleted = true;
                    (entity as IAuditable).Stamp(requestedBy);
                    var response = await container.ReplaceItemAsync(entity, entity.Id.ToString(), partitionKey, requestOptions, cancellationToken);
                    _partitionKeyValue = null;  // reset
                }
                else
                {
                    throw new Exception($"Entity {entity.Id} already deleted");
                }
            }
            catch (CosmosException ex)
            {
                _logger.LogError($"Entity with ID: {entity.Id} was not removed successfully - error details: {ex.Message}");
                throw;
            }
        }

        public virtual async Task DeleteAsync(TKey id, string partitionKeyValue, bool forceHardDelete = false, string requestedBy = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                var container = GetContainer();
                var partitionKey = new PartitionKey(partitionKeyValue);

                if (forceHardDelete || !typeof(ISoftDelete).IsAssignableFrom(typeof(TEntity)))
                {
                    var requestOptions = new ItemRequestOptions { EnableContentResponseOnWrite = false };
                    await container.DeleteItemAsync<TEntity>(id.ToString(), partitionKey, requestOptions, cancellationToken);
                    _partitionKeyValue = null;  // reset
                }
                else
                {
                    ItemResponse<TEntity> entityResult = await container.ReadItemAsync<TEntity>(id.ToString(), partitionKey);
                    var entity = entityResult.Resource;
                    var softDelete = entity as ISoftDelete;
                    if (entity == null)
                    {
                        throw new KeyNotFoundException($"Entity {id} not found");
                    }
                    else if (softDelete != null && !softDelete.IsDeleted)
                    {
                        var requestOptions = new ItemRequestOptions { IfMatchEtag = entity.ConcurrencyToken, EnableContentResponseOnWrite = false };
                        softDelete.IsDeleted = true;
                        (entity as IAuditable).Stamp(requestedBy);
                        await container.ReplaceItemAsync(entity, entity.Id.ToString(), partitionKey, requestOptions, cancellationToken);
                        _partitionKeyValue = null;  // reset
                    }
                    else
                    {
                        throw new Exception($"Entity {id} already deleted");
                    }
                }
            }
            catch (CosmosException ex)
            {
                _logger.LogError($"Entity with ID: {id} was not removed successfully - error details: {ex.Message}");
                throw;
            }
        }

        protected Container GetContainer()
        {
            var database = _client.GetDatabase(_configuration.DatabaseName);
            var container = database.GetContainer(_configuration.ContainerName);
            return container;
        }

        protected PartitionKey GetPartitionKey(TEntity entity)
        {
            if (string.IsNullOrEmpty(_partitionKeyValue))
            {
                if (!string.IsNullOrEmpty(entity.PartitionKey))
                {
                    _partitionKeyValue = entity.PartitionKey;
                }
                else
                {
                    throw new ApplicationException("Aggregate Root PartitionKey cannot be null");
                }
            }
            else if (string.IsNullOrEmpty(entity.PartitionKey))
            {
                entity.PartitionKey = _partitionKeyValue;
            }
            else if (entity.PartitionKey != _partitionKeyValue)
            {
                throw new ApplicationException("Entity PartitionKey ` Root PartitionKey");
            }

            PartitionKey partitionKey = new(_partitionKeyValue);

            return partitionKey;
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
        // ~CosmosCommandRepository()
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
    /// Adds, Update, AddOrUpdate, Delete an entity and saves an audit entity
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TAuditEntity"></typeparam>
    /// <typeparam name="TAuditKey"></typeparam>
    public class CosmosCommandRepository<TEntity, TKey, TAuditEntity, TAuditKey> : CosmosCommandRepository<TEntity, TKey>, INoSqlCommandRepository<TEntity, TKey, TAuditEntity, TAuditKey>
        where TEntity : class, IEntity<TKey>, IOptimisticConcurrency, IPartitionKey
        where TAuditEntity : NoSqlAudit<TAuditKey, TKey>, new()
    {
        public CosmosCommandRepository
        (
          ICosmosContainerConfiguration<TEntity> configuration,
          CosmosClient client,
          ILogger<CosmosCommandRepository<TEntity, TKey>> logger
        )
            : base(configuration, client, logger)
        {
        }

        public override async Task<TEntity> AddAsync(TEntity entity, string requestedBy = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                ItemResponse<TEntity> itemResponse;
                var container = GetContainer();
                var partitionKey = GetPartitionKey(entity);
                var batch = container.CreateTransactionalBatch(partitionKey);

                (entity as IAuditable).Stamp(requestedBy);
                batch.CreateItem(entity);

                var auditEntity = new TAuditEntity();
                auditEntity.SetAuditedEntity(entity);
                batch.CreateItem(auditEntity);

                using (TransactionalBatchResponse response = await batch.ExecuteAsync())
                {
                    if (response.IsSuccessStatusCode)
                    {
                        _partitionKeyValue = null;  // reset
                        return response.GetOperationResultAtIndex<TEntity>(0).Resource;
                    }
                    else
                    {
                        throw new InvalidOperationException($"{response.ErrorMessage} - AddAsync({JsonConvert.SerializeObject(entity)})");
                    }
                }
            }
            catch (CosmosException ex)
            {
                _logger.LogError($"New entity with ID: {entity.Id} was not added successfully - error details: {ex.Message}");
                throw;
            }
        }

        public override async Task<TEntity> UpdateAsync(TEntity entity, string requestedBy = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                var container = GetContainer();
                var id = entity.Id.ToString();
                var partitionKey = GetPartitionKey(entity);
                var batch = container.CreateTransactionalBatch(partitionKey);
                var requestOptions = new TransactionalBatchItemRequestOptions { IfMatchEtag = entity.ConcurrencyToken };

                (entity as IAuditable).Stamp(requestedBy);
                batch.ReplaceItem(id, entity, requestOptions);

                var auditEntity = new TAuditEntity();
                auditEntity.SetAuditedEntity(entity);
                batch.CreateItem(auditEntity);

                using (TransactionalBatchResponse response = await batch.ExecuteAsync())
                {
                    if (response.IsSuccessStatusCode)
                    {
                        _partitionKeyValue = null;  // reset
                        return response.GetOperationResultAtIndex<TEntity>(0).Resource;
                    }
                    else
                    {
                        throw new InvalidOperationException($"{response.ErrorMessage} - UpdateAsync({JsonConvert.SerializeObject(entity)})");
                    }
                }
            }
            catch (CosmosException ex)
            {
                _logger.LogError($"Entity with ID: {entity.Id} was not updated successfully - error details: {ex.Message}");
                throw;
            }
        }

        public override async Task<TEntity> AddOrUpdateAsync(TEntity entity, string requestedBy = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                ItemResponse<TEntity> itemResponse;
                var container = GetContainer();
                var partitionKey = GetPartitionKey(entity);
                var batch = container.CreateTransactionalBatch(partitionKey);
                var requestOptions = new TransactionalBatchItemRequestOptions { IfMatchEtag = entity.ConcurrencyToken };

                (entity as IAuditable).Stamp(requestedBy);
                batch.UpsertItem(entity, requestOptions);

                var auditEntity = new TAuditEntity();
                auditEntity.SetAuditedEntity(entity);
                batch.CreateItem(auditEntity);

                using (TransactionalBatchResponse response = await batch.ExecuteAsync())
                {
                    if (response.IsSuccessStatusCode)
                    {
                        _partitionKeyValue = null;  // reset
                        return response.GetOperationResultAtIndex<TEntity>(0).Resource;
                    }
                    else
                    {
                        throw new InvalidOperationException($"{response.ErrorMessage} - AddOrUpdateAsync({JsonConvert.SerializeObject(entity)})");
                    }
                }

                return itemResponse.Resource;
            }
            catch (CosmosException ex)
            {
                _logger.LogError($"New entity with ID: {entity.Id} was not added successfully - error details: {ex.Message}");
                throw;
            }
        }

        public override async Task DeleteAsync(TEntity entity, bool forceHardDelete = false, string requestedBy = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                var container = GetContainer();
                var partitionKey = GetPartitionKey(entity);
                var batch = container.CreateTransactionalBatch(partitionKey);

                var softDelete = entity as ISoftDelete;
                if (forceHardDelete || softDelete == null)
                {
                    var requestOptions = new TransactionalBatchItemRequestOptions { EnableContentResponseOnWrite = false };
                    batch.DeleteItem(entity.Id.ToString(), requestOptions);
                }
                else if (softDelete != null && !softDelete.IsDeleted)
                {
                    var requestOptions = new TransactionalBatchItemRequestOptions { IfMatchEtag = entity.ConcurrencyToken, EnableContentResponseOnWrite = false };
                    softDelete.IsDeleted = true;
                    (entity as IAuditable).Stamp(requestedBy);
                    batch.ReplaceItem(entity.Id.ToString(), entity, requestOptions);
                }
                else
                {
                    throw new Exception($"Entity {entity.Id} already deleted");
                }

                var auditEntity = new TAuditEntity();
                auditEntity.SetAuditedEntity(entity, true);
                batch.CreateItem(auditEntity);

                using (TransactionalBatchResponse response = await batch.ExecuteAsync())
                {
                    if (response.IsSuccessStatusCode)
                    {
                        _partitionKeyValue = null;  // reset
                    }
                    else
                    {
                        throw new InvalidOperationException($"{response.ErrorMessage} - DeleteAsync({JsonConvert.SerializeObject(entity)})");
                    }
                }
            }
            catch (CosmosException ex)
            {
                _logger.LogError($"Entity with ID: {entity.Id} was not removed successfully - error details: {ex.Message}");
                throw;
            }
        }

        public override async Task DeleteAsync(TKey id, string partitionKeyValue, bool forceHardDelete = false, string requestedBy = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                var container = GetContainer();
                var partitionKey = new PartitionKey(partitionKeyValue);
                var batch = container.CreateTransactionalBatch(partitionKey);

                ItemResponse<TEntity> entityResult = await container.ReadItemAsync<TEntity>(id.ToString(), partitionKey);
                var entity = entityResult.Resource;

                if (entity != null)
                {
                    var softDelete = entity as ISoftDelete;
                    if (forceHardDelete || softDelete == null)
                    {
                        var requestOptions = new TransactionalBatchItemRequestOptions { EnableContentResponseOnWrite = false };
                        batch.DeleteItem(id.ToString(), requestOptions);
                    }
                    else if (softDelete != null && !softDelete.IsDeleted)
                    {
                        var requestOptions = new TransactionalBatchItemRequestOptions { IfMatchEtag = entity.ConcurrencyToken, EnableContentResponseOnWrite = false };
                        softDelete.IsDeleted = true;
                        (entity as IAuditable).Stamp(requestedBy);
                        batch.ReplaceItem<TEntity>(entity.Id.ToString(), entity, requestOptions);
                    }
                    else
                    {
                        throw new Exception($"Entity {id} already deleted");
                    }

                    var auditEntity = new TAuditEntity();
                    auditEntity.SetAuditedEntity(entity, true);
                    batch.CreateItem(auditEntity);

                    using (TransactionalBatchResponse response = await batch.ExecuteAsync())
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            _partitionKeyValue = null;  // reset
                        }
                        else
                        {
                            throw new InvalidOperationException($"{response.ErrorMessage} - DeleteAsync({JsonConvert.SerializeObject(entity)})");
                        }
                    }
                }
            }
            catch (CosmosException ex)
            {
                _logger.LogError($"Entity with ID: {id} was not removed successfully - error details: {ex.Message}");
                throw;
            }
        }
    }
}
