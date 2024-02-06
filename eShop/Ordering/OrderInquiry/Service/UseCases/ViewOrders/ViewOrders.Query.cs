using FluentValidation.Results;
using IoCloud.Shared.MessageHandling.Abstractions;
using Newtonsoft.Json;

namespace eShop.Ordering.OrderInquiry.Service.UseCases.ViewOrders
{
    public class ViewOrdersQuery : IQuery<IEnumerable<ViewOrdersReply>>
    {
        public ViewOrdersQuery(string emailAddress)
        {
            EmailAddress = emailAddress;
        }

        public string EmailAddress { get; set; }

        [JsonIgnore]
        public ValidationResult ValidationResult { get; set; }

        public static bool TryParse(string? emailAddress, out ViewOrdersQuery? query)
        {
            if (emailAddress?.Length > 0)
            {
                query = new ViewOrdersQuery(emailAddress);
                return true;
            }
            query = null;
            return false;
        }
    }
}
