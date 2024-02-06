namespace vesa.Identity.Abstractions
{
    public interface ICustomerProfileManager
    {
        ICustomerProfile GetCustomerProfile(string customerNumber);
    }
}
