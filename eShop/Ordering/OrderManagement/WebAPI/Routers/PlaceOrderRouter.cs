using eShop.Ordering.OrderManagement.Service.UseCases.PlaceOrder;
using IoCloud.Shared.HttpRouting.Abstractions;
using IoCloud.Shared.HttpRouting.Infrastructure;
using IoCloud.Shared.MessageHandling.Abstractions;

namespace eShop.Ordering.OrderManagment.WebAPI.Routers
{
    public class PlaceOrderRouter : HttpPostRouter<PlaceOrderCommand, PlaceOrderReply>
    {
        private const string URL = "/orders/place";

        public PlaceOrderRouter
        (
            IServiceProvider serviceProvider,
            IRequestedByResolver requestedByResolver,
            IIocMediatorFactory mediatorFactory
        )
            : base(serviceProvider, requestedByResolver, mediatorFactory, URL)
        {
        }
    }
}
