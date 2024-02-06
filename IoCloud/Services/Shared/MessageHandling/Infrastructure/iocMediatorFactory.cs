using MediatR;
using IoCloud.Shared.MessageHandling.Abstractions;

namespace IoCloud.Shared.MessageHandling.Infrastructure
{
    /// <summary>
    /// Creates instances of IIocMediator
    /// </summary>
    public class IocMediatorFactory : IIocMediatorFactory
    {
        public IIocMediator CreateMediator(ServiceFactory serviceFactory)
        {
            Mediator mediator = new(serviceFactory);
            return new IocMediator(mediator);
        }
    }
}
