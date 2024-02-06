using eShop.Ordering.OrderManagement.Data.Entities;
using FluentValidation.Results;
using IoCloud.Shared.MessageHandling.Abstractions;
using IoCloud.Shared.Messages;

namespace eShop.Ordering.OrderManagement.Service.UseCases.CancelOrder
{
    public class CancelOrderCommand : CloudEventMessagePayload, ICommand<CancelOrderReply>
    {
        public CancelOrderCommand() : base()
        {
            Header.Type = "eShop.ordering.orderManagement.cancelOrder";
            Header.Subject = $"https://ordering.eshop.com/orders/{this.OrderId}";
            Header.CorrelationId = Header.Id;
        }

        public Guid OrderId { get; set; }
        public ValidationResult ValidationResult { get; set; }
    }
}
