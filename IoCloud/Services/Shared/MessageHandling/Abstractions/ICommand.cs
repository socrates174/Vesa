using IoCloud.Shared.MessageHandling.Infrastructure;
using MediatR;

namespace IoCloud.Shared.MessageHandling.Abstractions
{
    public interface ICommand : ICommand<VoidReply>
    {
    }

    public interface ICommand<TReply> : IRequest<TReply>, IBaseCommand
    {
    }
}
