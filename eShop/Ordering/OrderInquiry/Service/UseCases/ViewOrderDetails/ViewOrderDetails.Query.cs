using FluentValidation.Results;
using IoCloud.Shared.MessageHandling.Abstractions;
using Newtonsoft.Json;

namespace eShop.Ordering.OrderInquiry.Service.UseCases.ViewOrderDetails
{
    public class ViewOrderDetailsQuery : IQuery<ViewOrderDetailsReply>
    {
        public ViewOrderDetailsQuery(Guid id)
        {
            Id = id;
        }

        public Guid Id { get; set; }

        [JsonIgnore]
        public ValidationResult ValidationResult { get; set; }

        public static bool TryParse(Guid? id, out ViewOrderDetailsQuery? query)
        {
            if (id != null)
            {
                query = new ViewOrderDetailsQuery(id.Value);
                return true;
            }
            query = null;
            return false;
        }
    }
}
