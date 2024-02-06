using MediatR;

namespace IoCloud.Shared.MessageHandling.Abstractions
{
    public interface IIocMediatorFactory
    {
        IIocMediator CreateMediator(ServiceFactory serviceFactory);
    }
}
