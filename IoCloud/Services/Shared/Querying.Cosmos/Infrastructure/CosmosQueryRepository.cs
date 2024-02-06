using IoCloud.Shared.Entity.Abstractions;
using IoCloud.Shared.Querying.Abstractions;
using IoCloud.Shared.Querying.NoSql.Abstractions;
using IoCloud.Shared.Settings.Abstractions;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;

namespace IoCloud.Shared.Querying.NoSql.Infrastructure
{
    /// <summary>
    /// Retrieve an entity or a list of entities or a Queryable of an entity
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    public class CosmosQueryRepository<TEntity> : CosmosQueryRepository<TEntity, Guid>, INoSqlQueryRepository<TEntity>, IQueryExistence<TEntity>
        where TEntity : class, IEntity
    {
        public CosmosQueryRepository
        (
            ICosmosContainerConfiguration<TEntity> configuration,
            CosmosClient client,
            ILogger<CosmosQueryRepository<TEntity>> logger
        )
            : base(configuration, client, logger)
        {
        }
    }

    public class CosmosQueryRepository<TEntity, TKey> : INoSqlQueryRepository<TEntity, TKey>, IQueryExistence<TEntity, TKey>
        where TEntity : class, IEntity<TKey>
    {

        private bool disposedValue;

        private readonly ICosmosContainerConfiguration<TEntity> _configuration;
        private readonly CosmosClient _client;
        private readonly ILogger<CosmosQueryRepository<TEntity, TKey>> _logger;

        public CosmosQueryRepository
        (
            ICosmosContainerConfiguration<TEntity> configuration,
            CosmosClient client,
            ILogger<CosmosQueryRepository<TEntity, TKey>> logger
        )
        {
            _configuration = configuration;
            _client = client;
            _logger = logger;
        }

        public async Task<bool> ExistsAsync(TKey id)
        {
            var entity = await GetAsync(id);
            return entity != null;
        }

        public async Task<TEntity> GetAsync(TKey id, bool includeSoftDeleted = false)
            => await GetAsync(id, id.ToString(), includeSoftDeleted);

        public async Task<TEntity> GetAsync(TKey id, string partitionKeyValue, bool includeSoftDeleted = false)
        {
            try
            {
                var container = GetContainer();
                ItemResponse<TEntity> entityResult = await container.ReadItemAsync<TEntity>(id.ToString(), new PartitionKey(partitionKeyValue));
                var entity = entityResult.Resource;
                if (entity is ISoftDelete && !includeSoftDeleted && ((ISoftDelete)entity).IsDeleted)
                {
                    entity = null;
                }
                return entity;
            }
            catch (CosmosException ex)
            {
                if (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return null;
                }
                else
                {
                    _logger.LogError($"Entity with ID: {id} was not retrieved successfully - error details: {ex.Message}");
                    throw;
                }
            }
        }

        public async Task<IReadOnlyList<TEntity>> GetListAsync
        (
            string queryText = "SELECT * FROM c",
            bool includeSoftDeleted = false,
            params (string Name, object Value)[] parameters
        )
        {
            var entityTypeName = typeof(TEntity).FullName;
            if (queryText.Contains("WHERE", StringComparison.InvariantCultureIgnoreCase))
            {
                queryText += $" and c.EntityType == \"{entityTypeName}\"";
            }
            else
            {
                queryText += " where c.EntityType == \"{entityTypeName}\"";
            }

            if (typeof(ISoftDelete).IsAssignableFrom(typeof(TEntity)) && !includeSoftDeleted)
            {
                if (queryText.Contains("WHERE", StringComparison.InvariantCultureIgnoreCase))
                {
                    queryText += " and c.IsDeleted == false";
                }
                else
                {
                    queryText += " where c.IsDeleted == false";
                }
            }

            try
            {
                var container = GetContainer();
                QueryDefinition queryDefinition = (parameters ?? Array.Empty<(string Name, object Value)>())
                                                    .Aggregate
                                                    (
                                                        new QueryDefinition(queryText),
                                                        (q, param) => q.WithParameter(param.Name, param.Value)
                                                    );
                FeedIterator<TEntity> iterator = container.GetItemQueryIterator<TEntity>(queryDefinition);
                List<TEntity> entities = new();

                try
                {
                    while (iterator.HasMoreResults)
                    {
                        var feedResponse = await iterator.ReadNextAsync();
                        foreach (var entity in feedResponse)
                        {
                            entities.Add(entity);
                        }
                    }
                }
                finally
                {
                    if (iterator != null)
                    {
                        iterator.Dispose();
                    }
                }

                return entities;
            }
            catch (CosmosException ex)
            {
                _logger.LogError($"Entities was not retrieved successfully - error details: {ex.Message}");
                throw;
            }
        }

        public IQueryable<TEntity> GetQuery()
        {
            try
            {
                var entityTypeName = typeof(TEntity).FullName;
                var container = GetContainer();
                return container.GetItemLinqQueryable<TEntity>().Where(entity => entity.EntityType == entityTypeName);
            }
            catch (CosmosException ex)
            {
                _logger.LogError($"Entities was not retrieved successfully - error details: {ex.Message}");
                throw;
            }
        }

        private Container GetContainer()
        {
            var database = _client.GetDatabase(_configuration.DatabaseName);
            var container = database.GetContainer(_configuration.ContainerName);
            return container;
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
        // ~CosmosQueryRepository()
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