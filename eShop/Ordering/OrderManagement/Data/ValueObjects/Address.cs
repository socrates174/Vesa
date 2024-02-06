namespace eShop.Ordering.OrderManagement.Data.ValueObjects
{
    public class Address
    {
        public string Unit { get; set; }
        public string StreetNumber { get; set; }
        public string StreetName { get; set; }
        public string City { get; set; }
        public string Province { get; set; }
        public string PostalCode { get; set; }
        public override string ToString() => $"{Unit}{(string.IsNullOrEmpty(Unit) ? string.Empty : "-")}{StreetNumber} {StreetName}, {City}, {Province} {PostalCode}";
    }
}
