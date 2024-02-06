using eShop.Ordering.OrderManagement.Data.Enums;
using IoCloud.Shared.Domain;
using IoCloud.Shared.MessageHandling.Abstractions;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations.Schema;

namespace eShop.Ordering.OrderManagement.Service.UseCases.CancelOrder
{
    public class CancelOrderDomain : IAggregateRoot, ICommandProcessor<CancelOrderCommand>, ICancelOrderDomain
    {
        public Guid Id { get; private set; }
        public OrderStatus Status { get; private set; }
        public DateTimeOffset? DateCancelled { get; private set; }

        [JsonIgnore]
        [NotMapped]
        public IList<IDomainMessage> DomainMessages { get; } = new List<IDomainMessage>();

        [NotMapped]
        public string DomainVersion { get; private set; } = "1.0";

        public string EntityType => this.GetType().FullName;


        public void Process(CancelOrderCommand command)
        {
            Status = OrderStatus.Cancelled;
            DateCancelled = DateTimeOffset.Now;

            var orderCancelledEvent = new OrderCancelledEvent(Id, Status, DateCancelled.Value);
            orderCancelledEvent.Header.Subject = command.Header.Subject;
            orderCancelledEvent.Header.CorrelationId = command.Header.CorrelationId;
            orderCancelledEvent.Header.RequestedBy = command.Header.RequestedBy;
            DomainMessages.Add(orderCancelledEvent);
        }
    }

    public interface ICancelOrderDomain
    {
        IList<IDomainMessage> DomainMessages { get; }

        void Process(CancelOrderCommand command);
    }
}
