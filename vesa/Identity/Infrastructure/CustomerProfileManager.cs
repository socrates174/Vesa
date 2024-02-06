using vesa.Identity.Abstractions;

namespace vesa.Identity.Infrastructure
{
    public class CustomerProfileManager : ICustomerProfileManager
    {
        public ICustomerProfile GetCustomerProfile(string customerNumber)
        {
            return new CustomerProfile(customerNumber, "Harsh J.", "123 Main St, Calgary, Alberta");
        }
    }
}
