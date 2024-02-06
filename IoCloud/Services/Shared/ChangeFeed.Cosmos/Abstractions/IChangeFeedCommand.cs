using IoCloud.Shared.MessageHandling.Abstractions;
using IoCloud.Shared.MessageHandling.Infrastructure;

namespace IoCloud.Shared.ChangeFeed.Abstractions
{
    public interface IChangeFeedCommand<TChangedEntity, TRootEntity> : IChangeFeedCommand<TChangedEntity>
    {
    }

    public interface IChangeFeedCommand<TChangedEntity> : IChangeFeedCommand
    {
        TChangedEntity Data { get; set; }
    }

    public interface IChangeFeedCommand : ICommand<VoidReply>
    {
    }
}
