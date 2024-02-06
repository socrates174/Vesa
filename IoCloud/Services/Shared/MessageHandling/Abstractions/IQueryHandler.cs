using MediatR;

namespace IoCloud.Shared.MessageHandling.Abstractions
{
    public interface IQueryHandler<in TQuery, TReply> : IRequestHandler<TQuery, TReply>
        where TQuery : IQuery<TReply>
    {
    }
}
