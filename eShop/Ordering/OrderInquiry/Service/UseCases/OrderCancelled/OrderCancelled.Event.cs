using IoCloud.Shared.MessageHandling.Abstractions;
using IoCloud.Shared.Messages;

namespace eShop.Ordering.OrderInquiry.Service.UseCases.OrderCancelled
{
    public class OrderCancelledEvent : CloudEventMessagePayload, IEvent
    {
        public Guid OrderId { get; set; }
        public string Status { get; set; }
        public DateTimeOffset DateCancelled { get; set; }
    }
}
