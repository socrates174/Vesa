namespace IoCloud.Shared.MessageHandling.Abstractions
{
    public interface IIocPublisher
    {
        Task Publish(object notification, CancellationToken cancellationToken = default(CancellationToken));
    }
}
