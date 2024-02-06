using eShop.Ordering.OrderManagement.Data.Entities;

namespace eShop.Ordering.OrderManagement.Data.ValueObjects
{
    public class OrderItem
    {
        public Product Product { get; set; }
        public int Quantity { get; set; }
    }
}
