﻿using IoCloud.Shared.Entity.Abstractions;
using IoCloud.Shared.Messages;

namespace eShop.Ordering.OrderManagement.Data.Entities
{
    public class OutboxEvent : CloudEventMessage, IPartitionKey
    {
        public string PartitionKey { get; set; }
    }
}
