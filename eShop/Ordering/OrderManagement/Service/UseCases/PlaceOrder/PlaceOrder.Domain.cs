using eShop.Ordering.OrderManagement.Data.Entities;
using eShop.Ordering.OrderManagement.Data.Enums;
using IoCloud.Shared.Domain;
using IoCloud.Shared.MessageHandling.Abstractions;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations.Schema;

namespace eShop.Ordering.OrderManagement.Service.UseCases.PlaceOrder
{
    public class PlaceOrderDomain : Order, IAggregateRoot, ICommandProcessor<PlaceOrderCommand>, IPlaceOrderDomain
    {
        [JsonIgnore]
        [NotMapped]
        public IList<IDomainMessage> DomainMessages { get; } = new List<IDomainMessage>();

        [NotMapped]
        public string DomainVersion { get; private set; } = "1.0";

        public string EntityType => this.GetType().FullName;

        public void Process(PlaceOrderCommand command)
        {
            Id = GenerateId();
            command.Header.Subject = $"https://ordering.eshop.com/orders/{this.Id}";
            DateOrdered = DateTimeOffset.Now;
            Items = command.Items;
            Buyer = command.Buyer;
            ShippingAddress = command.ShippingAddress;
            Payment = command.Payment;
            Status = OrderStatus.Placed;

            var orderPlacedEvent = new OrderPlacedEvent(Id, DateOrdered, Items, Buyer, ShippingAddress, Payment, Status, ExpectedDelivery);
            orderPlacedEvent.Header.Subject = command.Header.Subject;
            orderPlacedEvent.Header.CorrelationId = command.Header.CorrelationId;
            orderPlacedEvent.Header.RequestedBy = command.Header.RequestedBy;
            DomainMessages.Add(orderPlacedEvent);
        }

        private Guid GenerateId()
        {
            return Guid.NewGuid();
        }
    }

    public interface IPlaceOrderDomain
    {
        IList<IDomainMessage> DomainMessages { get; }

        void Process(PlaceOrderCommand command);
    }
}
