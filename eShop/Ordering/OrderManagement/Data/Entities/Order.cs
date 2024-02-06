using eShop.Ordering.OrderManagement.Data.Enums;
using eShop.Ordering.OrderManagement.Data.ValueObjects;
using IoCloud.Shared.Entity.Infrastructure;

namespace eShop.Ordering.OrderManagement.Data.Entities
{
    public class Order : NoSqlEntity
    {
        public DateTimeOffset DateOrdered { get; set; }
        public IList<OrderItem> Items { get; protected set; } = new List<OrderItem>();
        public Buyer Buyer { get; protected set; }
        public Address ShippingAddress { get; protected set; }
        public Payment Payment { get; protected set; }
        public OrderStatus Status { get; protected set; }
        public DateTimeOffset ExpectedDelivery { get; protected set; }
        public DateTimeOffset? DateCancelled { get; protected set; }
    }
}
