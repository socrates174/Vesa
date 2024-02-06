namespace eShop.Ordering.OrderManagement.Service.UseCases.PlaceOrder
{
    public class PlaceOrderReply
    {
        public Guid OrderId { get; set; }
        public DateTimeOffset ExpectedDelivery { get; set; }
    }
}
