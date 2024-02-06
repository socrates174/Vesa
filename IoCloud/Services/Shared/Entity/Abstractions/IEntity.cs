namespace IoCloud.Shared.Entity.Abstractions
{
    public interface IEntity : IEntity<Guid>
    {
    }

    public interface IEntity<TKey> : IBaseEntity
    {
        TKey Id { get; set; }
    }
}
