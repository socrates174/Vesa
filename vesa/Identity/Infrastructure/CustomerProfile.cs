using vesa.Identity.Abstractions;

namespace vesa.Identity.Infrastructure
{
    public class CustomerProfile : ICustomerProfile
    {
        public CustomerProfile(string customerNumber, string customerName, string address)
        {
            CustomerNumber = customerNumber;
            CustomerName = customerName;
            Address = address;
        }

        public string CustomerNumber { get; init; }
        public string CustomerName { get; init; }
        public string Address { get; init; }
    }
}
