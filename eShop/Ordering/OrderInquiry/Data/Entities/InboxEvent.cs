using IoCloud.Shared.Entity.Abstractions;
using IoCloud.Shared.Messages;

namespace eShop.Ordering.OrderInquiry.Data.Entities
{
    public class InboxEvent : CloudEventMessage, IPartitionKey
    {
        public string PartitionKey { get; set; }
    }
}
