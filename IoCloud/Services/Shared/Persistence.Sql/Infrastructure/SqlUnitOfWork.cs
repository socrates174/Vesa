using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using IoCloud.Shared.Entity.Abstractions;
using AutoMapper;
using IoCloud.Shared.Persistence.Sql.Abstractions;

namespace IoCloud.Shared.Persistence.Sql.Infrastructure
{
    public class SqlUnitOfWork : ISqlUnitOfWork
    {
        private readonly DbContext _dbContext;
        private readonly IMapper _mapper;
        private bool disposedValue;

        public SqlUnitOfWork(DbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        protected SqlUnitOfWork() : base()
        {
        }

        public ISqlCommandRepository<TEntity> CreateRepository<TEntity>() 
            where TEntity : class
        {
            return new SqlCommandRepository<TEntity>(_dbContext, _mapper);
        }

        public ISqlCommandRepository<TEntity, TKey> CreateRepository<TEntity, TKey>()
            where TEntity : class
        {
            return new SqlCommandRepository<TEntity, TKey>(_dbContext, _mapper);
        }

        public async Task<int> CommitAsync(string requestedBy, CancellationToken cancellationToken = default)
        {
            foreach (var entityEntry in _dbContext.ChangeTracker.Entries())
            {
                if (entityEntry.State == EntityState.Added)
                {
                    if (entityEntry.Entity is IAuditable auditable)
                    {
                        auditable.CreatedBy = requestedBy;
                        auditable.CreatedOn = DateTimeOffset.Now;
                        auditable.UpdatedBy = auditable.CreatedBy;
                        auditable.UpdatedOn = auditable.CreatedOn;
                    }
                }
                else if (entityEntry.State == EntityState.Modified)
                {
                    if (entityEntry.Entity is IAuditable auditable)
                    {
                        auditable.UpdatedBy = requestedBy;
                        auditable.UpdatedOn = DateTimeOffset.Now;
                    }
                }
                else if (entityEntry.State == EntityState.Deleted)
                {
                    if (entityEntry.Entity is IAuditable auditable)
                    {
                        auditable.UpdatedBy = requestedBy;
                        auditable.UpdatedOn = DateTimeOffset.Now;
                    }
                    if (entityEntry.Entity is ISoftDelete softDelete)
                    {
                        softDelete.IsDeleted = true;
                    }
                }
            }
            return await _dbContext.SaveChangesAsync(cancellationToken); 
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
        // ~SqlUnitOfWork()
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
