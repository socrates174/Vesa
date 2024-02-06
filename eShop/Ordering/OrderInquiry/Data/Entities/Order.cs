using IoCloud.Shared.Entity.Infrastructure;

namespace eShop.Ordering.OrderInquiry.Data.Entities
{
    public class Order : NoSqlEntity
    {
        public DateTimeOffset DateOrdered { get; set; }
        public IList<(string Item, int Quantity)> Items { get; set; } = new List<(string Item, int Quantity)>();
        public string EmailAddress { get; set; }
        public string ShippingAddress { get; set; }
        public decimal PaymentAmount { get; set; }
        public string Status { get; set; }
        public DateTimeOffset ExpectedDelivery { get; set; }
    }
}
