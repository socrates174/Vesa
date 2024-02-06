using IoCloud.Shared.Entity.Abstractions;
using Newtonsoft.Json;
using System.Dynamic;

namespace IoCloud.Shared.Messages
{
    /// <summary>
    /// Common attributes of a Cloud Event and EventGridEvent schema
    /// RequestedBy, CorrelationId, EntityType, ConcurrencyToken are IoCloud add-on properties
    /// </summary>
    public abstract class Message : IMessage, IOptimisticConcurrency
    {
        public Message()
        {
            Id = Guid.NewGuid().ToString();
        }

        public virtual string Id { get; set; }
        public virtual string Subject { get; set; }
        public ExpandoObject Data { get; set; }
        public string? RequestedBy { get; set; }
        public string CorrelationId { get; set; }
        public string EntityType => this.GetType().FullName;

        [JsonProperty(PropertyName = "_etag")]
        public virtual string ConcurrencyToken { get; set; }
        public virtual int Sequence { get; set; } = 0;

    }
}
