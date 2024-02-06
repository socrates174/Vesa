namespace IoCloud.Shared.Persistence.Sql.Abstractions
{
    public interface ISqlUnitOfWork : IDisposable
    {
        ISqlCommandRepository<TEntity> CreateRepository<TEntity>() where TEntity : class;
        ISqlCommandRepository<TEntity, TKey> CreateRepository<TEntity, TKey>() where TEntity : class;
        Task<int> CommitAsync(string requestedBy = null, CancellationToken cancellationToken = default(CancellationToken));
    }
}
