namespace IoCloud.Shared.Querying.Abstractions
{
    public interface IQueryExistence<TEntity> : IQueryExistence<TEntity, Guid>
        where TEntity : class
    {
    }

    public interface IQueryExistence<TEntity, TKey>
        where TEntity : class
    {
        Task<bool> ExistsAsync(TKey key);
    }
}
