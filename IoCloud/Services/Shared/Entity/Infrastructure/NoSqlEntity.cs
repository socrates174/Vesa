using Newtonsoft.Json;
using IoCloud.Shared.Entity.Abstractions;

namespace IoCloud.Shared.Entity.Infrastructure
{
    /// <summary>
    /// Base class that all no SQL entities should inherit from
    /// </summary>
    public class NoSqlEntity : NoSqlEntity<Guid>, IEntity
    {
        public NoSqlEntity()
        {
            Id = Guid.NewGuid();
        }

        public NoSqlEntity(Guid id) : base(id)
        {
        }
    }

    public class NoSqlEntity<TKey> : IEntity<TKey>, IOptimisticConcurrency, IPartitionKey
    {
        protected TKey _id;
        protected string _partitionKey;

        public NoSqlEntity()
        {
            Id = default;
        }

        public NoSqlEntity(TKey id)
        {
            Id = id;
        }

        public virtual TKey Id
        {
            get => _id;
            set
            {
                _id = value;
                _partitionKey = _id.ToString();
            }
        }

        [JsonProperty("_etag")]
        public virtual string ConcurrencyToken { get; set; }

        public virtual string PartitionKey
        {
            get => _partitionKey;
            set
            {
                if (_id.Equals(default(TKey)))
                {
                    _partitionKey = value;
                }
            }
        }

        public string EntityType => GetType().FullName;
    }
}
