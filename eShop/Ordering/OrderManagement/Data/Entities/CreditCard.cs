using eShop.Ordering.OrderManagement.Data.Enums;

namespace eShop.Ordering.OrderManagement.Data.Entities
{
    public class CreditCard
    {
        public CreditCardType Type { get; set; }
        public int CardNumber { get; set; }
        public string Expiry { get; set; }
    }
}
