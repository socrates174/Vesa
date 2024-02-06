namespace eShop.Ordering.OrderInquiry.Service.UseCases.ViewOrders
{
    public class ViewOrdersReply
    {
        public ViewOrdersReply(Guid id, DateTimeOffset dateOrdered, decimal paymentAmount)
        {
            Id = id;
            DateOrdered = dateOrdered;
            PaymentAmount = paymentAmount;
        }

        public Guid Id { get; set; }
        public DateTimeOffset DateOrdered { get; set; }
        public decimal PaymentAmount { get; set; }
    }
}
