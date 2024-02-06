namespace IoCloud.Shared.Entity.Abstractions
{
    public interface INoSqlAudit
    {
        string AuditedEntityData { get; set; }
        string AuditedEntityType { get; set; }
    }
}
