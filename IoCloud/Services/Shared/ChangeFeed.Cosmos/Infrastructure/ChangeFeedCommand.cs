using FluentValidation.Results;
using IoCloud.Shared.ChangeFeed.Abstractions;

namespace IoCloud.Shared.ChangeFeed.Infrastructure
{
    /// <summary>
    /// A Command that contains an entity (detected by the Cosmos Change Feed) to be dispatched to a command handler that performs some action on that entity
    /// TRootEntity is root entity whose partition the TEntity resides in
    /// </summary>
    /// <typeparam name="TChangedEntity"></typeparam>
    /// <typeparam name="TRootEntity"></typeparam>
    public class ChangeFeedCommand<TChangedEntity, TRootEntity> : ChangeFeedCommand<TChangedEntity>, IChangeFeedCommand<TChangedEntity, TRootEntity>
    {
        public ChangeFeedCommand(TChangedEntity data) : base(data)
        {
        }
    }

    /// <summary>
    /// A Command that contains an entity (detected by the Cosmos Change Feed) to be dispatched to a command handler that performs some action on that entity
    /// </summary>
    /// <typeparam name="TChangedEntity"></typeparam>
    public class ChangeFeedCommand<TChangedEntity> : IChangeFeedCommand<TChangedEntity>
    {
        public ChangeFeedCommand(TChangedEntity data)
        {
            Data = data;
        }

        public TChangedEntity Data { get; set; }
        public ValidationResult ValidationResult { get; set;}
    }
}
