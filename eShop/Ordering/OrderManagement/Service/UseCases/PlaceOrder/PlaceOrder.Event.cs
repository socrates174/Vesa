using eShop.Ordering.OrderManagement.Data.Entities;
using eShop.Ordering.OrderManagement.Data.Enums;
using eShop.Ordering.OrderManagement.Data.ValueObjects;
using IoCloud.Shared.Domain;
using IoCloud.Shared.MessageHandling.Abstractions;
using IoCloud.Shared.Messages;

namespace eShop.Ordering.OrderManagement.Service.UseCases.PlaceOrder
{
    public class OrderPlacedEvent : CloudEventMessagePayload, IDomainMessage, IEvent
    {
        public OrderPlacedEvent() : base()
        {
            Header.Type = "eShop.ordering.orderManagement.orderPlaced";
            Header.Source = "https://eShop.com/orders/place";
        }

        public OrderPlacedEvent
        (
            Guid orderId, 
            DateTimeOffset dateOrdered, 
            IList<OrderItem> items, 
            Buyer buyer, 
            Address shippingAddress,
            Payment payment,
            OrderStatus status, 
            DateTimeOffset expectedDelivery
       ) 
            : this()
        {
            OrderId = orderId;
            DateOrdered = dateOrdered;
            Items = items.Select(i => (i.Product.Name, i.Quantity)).ToList();
            EmailAddress = buyer.EmailAddress;
            ShippingAddress = shippingAddress.ToString();
            PaymentAmount = payment.TotalAmount;
            Status = status.ToString();
            ExpectedDelivery = expectedDelivery;
        }

        public Guid OrderId { get; set; }
        public DateTimeOffset DateOrdered { get; set; }
        public IList<(string Item, int Quantity)> Items { get; set; } = new List<(string Item, int Quantity)>();
        public string EmailAddress { get; set; }
        public string ShippingAddress { get; set; }
        public decimal PaymentAmount { get; set; }
        public string Status { get; set; }
        public DateTimeOffset ExpectedDelivery { get; set; }
    }
}
