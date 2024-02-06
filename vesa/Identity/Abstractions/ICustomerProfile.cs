namespace vesa.Identity.Abstractions
{
    public interface ICustomerProfile
    {
        string CustomerNumber { get; init; }
        string CustomerName { get; init; }
        string Address { get; init; }
    }
}