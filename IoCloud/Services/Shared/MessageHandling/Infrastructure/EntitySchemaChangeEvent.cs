using IoCloud.Shared.MessageHandling.Abstractions;

namespace IoCloud.Shared.MessageHandling.Infrastructure
{
    public class EntitySchemaChangeEvent : IEvent
    {
        public string OldVersionTypeName { get; set; }
        public string NewVersionTypeName { get; set; }
    }
}
