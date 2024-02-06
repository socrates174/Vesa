using eShop.Ordering.OrderManagement.Data.Enums;
using IoCloud.Shared.Domain;
using IoCloud.Shared.MessageHandling.Abstractions;
using IoCloud.Shared.Messages;

namespace eShop.Ordering.OrderManagement.Service.UseCases.CancelOrder
{
    public class OrderCancelledEvent : CloudEventMessagePayload, IDomainMessage, IEvent
    {
        public OrderCancelledEvent() : base()
        {
            Header.Type = "eShop.ordering.orderManagement.orderCancelled";
            Header.Source = "https://eShop.com/orders/cancel";
        }

        public OrderCancelledEvent
        (
            Guid orderId, 
            OrderStatus status,
            DateTimeOffset dateCancelled
        )
            : this()
        {
            OrderId = orderId;
            Status = status.ToString();
            DateCancelled = dateCancelled;
        }

        public Guid OrderId { get; set; }
        public string Status { get; set; }
        public DateTimeOffset DateCancelled { get; set; }
    }
}