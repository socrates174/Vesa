using eShop.Ordering.OrderManagement.Data.Entities;
using eShop.Ordering.OrderManagement.Data.Enums;
using eShop.Ordering.OrderManagement.Data.ValueObjects;
using FluentValidation.Results;
using IoCloud.Shared.MessageHandling.Abstractions;
using IoCloud.Shared.Messages;

namespace eShop.Ordering.OrderManagement.Service.UseCases.PlaceOrder
{
    public class PlaceOrderCommand : CloudEventMessagePayload, ICommand<PlaceOrderReply>
    {
        public PlaceOrderCommand() : base()
        {
            Header.Type = "eShop.ordering.orderManagement.placeOrder";
            Header.CorrelationId = Header.Id;
        }

        public DateTimeOffset DateOrdered { get; set; }
        public IList<OrderItem> Items { get; set; } = new List<OrderItem>();
        public Buyer Buyer { get; set; }
        public Address ShippingAddress { get; set; }
        public Payment Payment { get; set; }
        public OrderStatus Status { get; set; }
        public ValidationResult ValidationResult { get; set; }
    }
}
