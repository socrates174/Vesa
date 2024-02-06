using IoCloud.Shared.MessageHandling.Infrastructure;
using MediatR;

namespace IoCloud.Shared.MessageHandling.Abstractions
{
    public interface ICommandHandler<TCommand> : ICommandHandler<TCommand, VoidReply>
        where TCommand : ICommand
    {
    }

    public interface ICommandHandler<TCommand, TReply> : IRequestHandler<TCommand, TReply>
        where TCommand : ICommand<TReply>
    {
    }
}