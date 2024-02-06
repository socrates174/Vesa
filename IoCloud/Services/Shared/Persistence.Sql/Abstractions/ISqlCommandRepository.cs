namespace IoCloud.Shared.Persistence.Sql.Abstractions
{
    public interface ISqlCommandRepository<TEntity> : ISqlCommandRepository<TEntity, Guid>
        where TEntity : class
    {
    }

    public interface ISqlCommandRepository<TEntity, TKey> : IDisposable
        where TEntity : class
    {
        void Add(TEntity entity);
        Task UpdateAsync(TEntity entity, params string[] navigationProperties);
        Task AddOrUpdateAsync(TEntity entity, params string[] navigationProperties);
        Task DeleteAsync(TEntity entity);
        Task DeleteAsync(TKey key);
        Task<int> SaveChangesAsyc(string requestedBy = null, CancellationToken cancellationToken = default(CancellationToken));
    }
}
