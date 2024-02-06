using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using IoCloud.Shared.Entity.Abstractions;
using IoCloud.Shared.Persistence.NoSql.Abstractions;
using IoCloud.Shared.Settings.Abstractions;

namespace IoCloud.Shared.Persistence.NoSql.Infrastructure
{
    /// <summary>
    /// Adds, Updates, AddOrUpdates, Deletes multiple entities in one transaction
    /// </summary>
    public class CosmosCommandUnitOfWork<TRootEntity> : INoSqlCommandUnitOfWork<TRootEntity>
        where TRootEntity : class
    {

        private bool disposedValue;

        private readonly ICosmosContainerConfiguration<TRootEntity> _configuration;
        private readonly CosmosClient _client;
        private readonly ILogger<CosmosCommandUnitOfWork<TRootEntity>> _logger;
        private string _unitOfWorkPartitionKeyValue;
        private TransactionalBatch _batch;
        private List<IBaseEntity> _batchEntities = new();
        private readonly DateTimeOffset _changedOn = DateTimeOffset.Now;

        public CosmosCommandUnitOfWork
        (
            ICosmosContainerConfiguration<TRootEntity> configuration,
            CosmosClient client,
            ILogger<CosmosCommandUnitOfWork<TRootEntity>> logger
        )
        {
            _configuration = configuration;
            _client = client;
            _logger = logger;
        }

        public void Add<TEntity>(TEntity entity) where TEntity : class, IEntity, IPartitionKey
        {
            Add<TEntity, Guid>(entity);
        }

        public void Update<TEntity>(TEntity entity) where TEntity : class, IEntity, IOptimisticConcurrency, IPartitionKey
        {
            Update<TEntity, Guid>(entity);
        }

        public void AddOrUpdate<TEntity>(TEntity entity) where TEntity : class, IEntity, IOptimisticConcurrency, IPartitionKey
        {
            AddOrUpdate<TEntity, Guid>(entity);
        }

        public void Delete<TEntity>(TEntity entity, bool forceHardDelete = false) where TEntity : class, IEntity, IOptimisticConcurrency, IPartitionKey
        {
            Delete<TEntity, Guid>(entity, forceHardDelete);
        }

        public async Task DeleteAsync<TEntity>(Guid id, string partitionKeyValue, bool forceHardDelete = false) where TEntity : class, IEntity, IOptimisticConcurrency, IPartitionKey
        {
            await DeleteAsync<TEntity, Guid>(id, partitionKeyValue, forceHardDelete);
        }

        public void Add<TEntity, TKey>(TEntity entity) where TEntity : class, IEntity<TKey>, IPartitionKey
        {
            // Assume the id of the first entity worked on is the partition key
            SetDefaultPartitionKey<TEntity, TKey>(entity);

            if (entity is IAuditable auditable)
            {
                auditable.CreatedOn = _changedOn;
                auditable.UpdatedOn = auditable.CreatedOn;
            }
            _batch.CreateItem<TEntity>(entity);
            _batchEntities.Add(entity);
        }

        public void Update<TEntity, TKey>(TEntity entity) where TEntity : class, IEntity<TKey>, IOptimisticConcurrency, IPartitionKey
        {
            // Assume the id of the first entity worked on is the partition key
            SetDefaultPartitionKey<TEntity, TKey>(entity);

            var requestOptions = new TransactionalBatchItemRequestOptions { IfMatchEtag = entity.ConcurrencyToken };
            if (entity is IAuditable auditable)
            {
                auditable.UpdatedOn = _changedOn;
            }
            _batch.ReplaceItem<TEntity>(entity.Id.ToString(), entity, requestOptions);
            _batchEntities.Add(entity);
        }

        public void AddOrUpdate<TEntity, TKey>(TEntity entity) where TEntity : class, IEntity<TKey>, IOptimisticConcurrency, IPartitionKey
        {
            // Assume the id of the first entity worked on is the partition key
            SetDefaultPartitionKey<TEntity, TKey>(entity);

            var requestOptions = new TransactionalBatchItemRequestOptions { IfMatchEtag = entity.ConcurrencyToken };
            if (entity is IAuditable auditable)
            {
                auditable.UpdatedOn = _changedOn;
                if (auditable.CreatedOn == default(DateTimeOffset))
                {
                    auditable.CreatedOn = auditable.UpdatedOn;
                }
            }
            _batch.UpsertItem<TEntity>(entity, requestOptions);
            _batchEntities.Add(entity);
        }

        public void Delete<TEntity, TKey>(TEntity entity, bool forceHardDelete = false) where TEntity : class, IEntity<TKey>, IOptimisticConcurrency, IPartitionKey
        {
            // Assume the id of the first entity worked on is the partition key
            SetDefaultPartitionKey<TEntity, TKey>(entity);

            var softDelete = entity as ISoftDelete;
            if (forceHardDelete || softDelete == null)
            {
                var requestOptions = new TransactionalBatchItemRequestOptions { EnableContentResponseOnWrite = false };
                _batch.DeleteItem(entity.Id.ToString(), requestOptions);
            }
            else if (softDelete != null && !softDelete.IsDeleted)
            {
                var requestOptions = new TransactionalBatchItemRequestOptions { IfMatchEtag = entity.ConcurrencyToken, EnableContentResponseOnWrite = false };
                if (entity is IAuditable auditable)
                {
                    auditable.UpdatedOn = _changedOn;
                }
                softDelete.IsDeleted = true;
                _batch.ReplaceItem<TEntity>(entity.Id.ToString(), entity, requestOptions);
                _batchEntities.Add(entity);
            }
            else
            {
                throw new Exception($"Entity {entity.Id} already deleted");
            }
        }

        public async Task DeleteAsync<TEntity, TKey>(TKey id, string partitionKeyValue, bool forceHardDelete = false) where TEntity : class, IEntity<TKey>, IOptimisticConcurrency, IPartitionKey
        {
            // Assume the id of the first entity worked on is the partition key
            SetDefaultPartitionKey<TEntity, TKey>(id, partitionKeyValue);

            if (forceHardDelete || !typeof(ISoftDelete).IsAssignableFrom(typeof(TEntity)))
            {
                var requestOptions = new TransactionalBatchItemRequestOptions { EnableContentResponseOnWrite = false };
                _batch.DeleteItem(id.ToString(), requestOptions);
            }
            else
            {
                var container = GetContainer();
                ItemResponse<TEntity> entityResult = await container.ReadItemAsync<TEntity>(id.ToString(), new PartitionKey(partitionKeyValue));
                var entity = entityResult.Resource;
                var softDelete = entity as ISoftDelete;
                if (entity == null)
                {
                    throw new KeyNotFoundException($"Entity {id} not found");
                }
                else if (softDelete != null && !softDelete.IsDeleted)
                {
                    var requestOptions = new TransactionalBatchItemRequestOptions { IfMatchEtag = entity.ConcurrencyToken, EnableContentResponseOnWrite = false };
                    if (entity is IAuditable auditable)
                    {
                        auditable.UpdatedOn = _changedOn;
                    }
                    softDelete.IsDeleted = true;
                    _batch.ReplaceItem<TEntity>(entity.Id.ToString(), entity, requestOptions);
                    _batchEntities.Add(entity);
                }
                else
                {
                    throw new Exception($"Entity {id} already deleted");
                }
            }
        }

        public async Task<IReadOnlyList<IBaseEntity>> CommitAsync(string requestedBy, CancellationToken cancellationToken = default(CancellationToken))
        {
            foreach(var entity in _batchEntities)
            {
                if (entity is IAuditable auditable)
                {
                    if (string.IsNullOrEmpty(auditable.CreatedBy))
                    {
                        auditable.CreatedBy = requestedBy;
                        auditable.UpdatedBy = auditable.CreatedBy;
                    }

                    if (auditable.CreatedOn == auditable.UpdatedOn)
                    {
                        auditable.CreatedBy = requestedBy;
                        auditable.UpdatedBy = auditable.CreatedBy;
                    }
                    else if (auditable.UpdatedOn > auditable.CreatedOn)
                    {
                        auditable.UpdatedBy = requestedBy;
                    }
                }
            }
            
            List<IBaseEntity> savedEntities = new();

            using (TransactionalBatchResponse batchResponse = await _batch.ExecuteAsync(cancellationToken))
            {
                if (!batchResponse.IsSuccessStatusCode)
                {
                    throw new Exception("TransactionalBatch execution failed");
                }
                else
                {
                    _unitOfWorkPartitionKeyValue = "";
                    if (batchResponse.Count > 0)
                    {
                        for (int i = 0; i < batchResponse.Count; i++)
                        {
                            StreamReader reader = new StreamReader(batchResponse[i].ResourceStream);
                            string json = reader.ReadToEnd();
                            var entity = JsonConvert.DeserializeObject(json, _batchEntities[i].GetType()) as IBaseEntity;
                            savedEntities.Add(entity);
                        }
                        _batchEntities.Clear();
                    }
                }
            }
            return savedEntities;
        }

        private Container GetContainer()
        {
            var database = _client.GetDatabase(_configuration.DatabaseName);
            var container = database.GetContainer(_configuration.ContainerName);
            return container;
        }

        private TransactionalBatch CreateBatch(string partitionKeyValue)
        {
            var container = GetContainer();
            PartitionKey partitionKey = new(partitionKeyValue);
            return container.CreateTransactionalBatch(partitionKey);
        }

        private void SetDefaultPartitionKey<TEntity, TKey>(TEntity entity)
            where TEntity : class, IEntity<TKey>, IPartitionKey
        {
            if (string.IsNullOrEmpty(entity.PartitionKey) && string.IsNullOrEmpty(_unitOfWorkPartitionKeyValue))
            {
                throw new ApplicationException("You must first involve the Aggregate Root in the transaction before including related entities.");
            }
            else if (string.IsNullOrEmpty(_unitOfWorkPartitionKeyValue))
            {
                if (!string.IsNullOrEmpty(entity.PartitionKey))
                {
                    _unitOfWorkPartitionKeyValue = entity.PartitionKey;
                    _batch = CreateBatch(_unitOfWorkPartitionKeyValue);
                }
                else
                {
                    throw new ApplicationException("Aggregate Root PartitionKey cannot be null");
                }
            }
            else if (string.IsNullOrEmpty(entity.PartitionKey))
            {
                entity.PartitionKey = _unitOfWorkPartitionKeyValue;
            }
            else if (entity.PartitionKey != _unitOfWorkPartitionKeyValue)
            {
                throw new ApplicationException("Entity PartitionKey does not match Aggregate Root PartitionKey");
            }
        }

        private void SetDefaultPartitionKey<TEntity, TKey>(TKey id, string partitionKeyValue)
            where TEntity : class, IEntity<TKey>, IPartitionKey
        {
            // Assume the id of the first entity worked on is the partition key
            if (string.IsNullOrEmpty(_unitOfWorkPartitionKeyValue))
            {
                _unitOfWorkPartitionKeyValue = partitionKeyValue;
                _batch = CreateBatch(_unitOfWorkPartitionKeyValue);
            }
            else if (_unitOfWorkPartitionKeyValue != partitionKeyValue)
            {
                throw new ApplicationException("Entity PartitionKey does not match Aggregate Root PartitionKey");
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
        // ~CosmosCommandUnitOfWork()
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
