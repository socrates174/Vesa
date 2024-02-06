using Microsoft.Azure.Cosmos;

namespace IoCloud.Shared.ChangeFeed.Abstractions
{
    public interface IChangeFeedProcessorFactory<TChangedEntity, TRootEntity> : IChangeFeedProcessorFactory<TChangedEntity>
    {
    }

    public interface IChangeFeedProcessorFactory<TChangedEntity>
    {
        ChangeFeedProcessorBuilder Builder { get; set; }
        ChangeFeedProcessor CreateProcessor();
    }
}