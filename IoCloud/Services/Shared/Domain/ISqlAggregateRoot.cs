namespace IoCloud.Shared.Domain
{
    public interface ISqlAggregateRoot : IAggregateRoot
    {
        string[] NavigationProperties { get; }

    }
}
