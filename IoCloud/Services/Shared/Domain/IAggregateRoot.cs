namespace IoCloud.Shared.Domain
{
    public interface IAggregateRoot : IAggregateRoot<Guid>
    {
    }

    public interface IAggregateRoot<TKey>
    {
        TKey Id { get; }
        IList<IDomainMessage> DomainMessages { get; }
        string DomainVersion { get; }
    }
}
