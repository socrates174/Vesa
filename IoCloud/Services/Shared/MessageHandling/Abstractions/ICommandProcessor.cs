namespace IoCloud.Shared.MessageHandling.Abstractions
{
    public interface ICommandProcessor<TCommand>
        where TCommand : IBaseCommand
    {
        void Process(TCommand command);
    }
}
