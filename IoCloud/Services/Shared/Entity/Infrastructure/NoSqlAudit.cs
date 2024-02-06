using Newtonsoft.Json;
using IoCloud.Shared.Entity.Abstractions;

namespace IoCloud.Shared.Entity.Infrastructure
{
    /// <summary>
    /// Base class that all no SQL audit entities should inherit from
    /// </summary>
    public abstract class NoSqlAudit : NoSqlAudit<Guid>
    {
        public NoSqlAudit() : base()
        {
        }
    }

    public abstract class NoSqlAudit<TAuditedKey> : NoSqlAudit<Guid, TAuditedKey>
    {
        public NoSqlAudit()
        {
            Id = Guid.NewGuid();
        }
    }

    public abstract class NoSqlAudit<TKey, TAuditedKey> : INoSqlAudit, IEntity<TKey>, IOptimisticConcurrency, IPartitionKey, ISoftDelete, IAuditable
    {
        public NoSqlAudit()
        {
            Id = default;
        }

        public void SetAuditedEntity(IEntity<TAuditedKey> auditedEntity, bool isDeleted = false)
        {
            AuditedEntityData = JsonConvert.SerializeObject(auditedEntity);
            AuditedEntityType = auditedEntity.GetType().FullName;
            PartitionKey = auditedEntity.Id.ToString();
            IsDeleted = isDeleted;
        }

        public virtual TKey Id { get; set; }

        public string AuditedEntityData { get; set; }
        public string AuditedEntityType { get; set; }

        [JsonProperty("_etag")]
        public virtual string ConcurrencyToken { get; set; }

        public virtual string PartitionKey { get; set; }

        public string EntityType => GetType().FullName;

        public bool IsDeleted { get; set; }
        public DateTimeOffset CreatedOn { get; set; } = DateTimeOffset.Now;
        public string CreatedBy { get; set; }
        public DateTimeOffset UpdatedOn { get; set; } = DateTimeOffset.Now;
        public string UpdatedBy { get; set; }

    }
}
