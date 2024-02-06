using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using IoCloud.Shared.Entity.Abstractions;

namespace IoCloud.Shared.Querying.Cosmos.Infrastructure
{
    public static class CosmosQueryableExtensions
    {
        public static async Task<IReadOnlyList<TEntity>> ToListAsync<TEntity>(this IQueryable<TEntity> query, bool includeSoftDeleted = false)
            where TEntity : class, IEntity
            => await query.ToListAsync<TEntity, Guid>(includeSoftDeleted);

        public async static Task<IReadOnlyList<TEntity>> ToListAsync<TEntity, TKey>(this IQueryable<TEntity> query, bool includeSoftDeleted = false)
            where TEntity : class, IEntity<TKey>
        {
            if (query is IOrderedQueryable<TEntity>)
            {
                query = query.Select(entity => entity);
            }

            if (typeof(ISoftDelete).IsAssignableFrom(typeof(TEntity)) && !includeSoftDeleted)
            {
                query = query.Where(entity => !((ISoftDelete)entity).IsDeleted);
            }

            try
            {
                FeedIterator<TEntity> iterator = query.ToFeedIterator<TEntity>();
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
                throw;
            }
        }

        public static async Task<IReadOnlyList<TEntity>> ToListAsync<TEntity>(this IQueryable<TEntity> query, int pageNumber, int pageSize, bool includeSoftDeleted = false)
            where TEntity : class, IEntity
            => await query.ToListAsync<TEntity, Guid>(pageNumber, pageSize, includeSoftDeleted);

        public static async Task<IReadOnlyList<TEntity>> ToListAsync<TEntity, TKey>(this IQueryable<TEntity> query, int pageNumber, int pageSize, bool includeSoftDeleted = false)
            where TEntity : class, IEntity<TKey>
        {
            if (pageNumber < 1)
            {
                throw new ArgumentException("pageNumber is one-based");
            }

            if (query is IOrderedQueryable<TEntity>)
            {
                query = query.Select(entity => entity);
            }

            if (typeof(ISoftDelete).IsAssignableFrom(typeof(TEntity)) && !includeSoftDeleted)
            {
                query = query.Where(entity => !((ISoftDelete)entity).IsDeleted);
            }

            query = query.Skip(pageSize * (pageNumber - 1)).Take(pageSize);

            try
            {
                FeedIterator<TEntity> iterator = query.ToFeedIterator<TEntity>();
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
                throw;
            }
        }
    }
}