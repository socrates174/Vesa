using System.ComponentModel.DataAnnotations;
using IoCloud.Shared.Entity.Abstractions;

namespace IoCloud.Shared.Entity.Infrastructure
{
    /// <summary>
    /// Base class that all SQL entities should inherit from
    /// </summary>
    public class SqlEntity : SqlEntity<Guid>, IEntity
    {
        public SqlEntity()
        {
            Id = Guid.NewGuid();
        }

        public SqlEntity(Guid id) : base(id)
        {
        }
    }

    public class SqlEntity<TKey> : IEntity<TKey>, IOptimisticConcurrency
    {
        protected TKey _id;

        public SqlEntity()
        {
            Id = default;
        }

        public SqlEntity(TKey id)
        {
            Id = id;
        }

        public virtual TKey Id { get; set; }

        [ConcurrencyCheck]
        public virtual string ConcurrencyToken { get; set; }

        public string EntityType => GetType().FullName;
    }
}
