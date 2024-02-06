namespace IoCloud.Shared.ChangeFeed.Abstractions
{
    public interface IChangeFeedHandler<TChangedEntity, TRootEntity> : IChangeFeedHandler<TChangedEntity>
    {
    }

    public interface IChangeFeedHandler<TChangedEntity>
    {
        Task HandleChangeAsync(TChangedEntity item, CancellationToken cancellationToken);
    }
}
