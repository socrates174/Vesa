namespace IoCloud.Shared.MessageHandling.Abstractions
{
    public interface IIocSender
    {
        Task<object?> Send(object request, CancellationToken cancellationToken = default(CancellationToken));
    }
}
