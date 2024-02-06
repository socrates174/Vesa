namespace eShop.Ordering.OrderManagement.Data.Entities
{
    public class Payment
    {
        public string TransactionId { get; set; }
        public CreditCard CreditCard { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
