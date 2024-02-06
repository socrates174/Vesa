using IoCloud.Shared.Settings.Abstractions;

namespace IoCloud.Shared.Settings.Infrastructure
{
    public class MessageMapping : IMessageMapping
    {
        public MessageMapping()
        {
        }

        public MessageMapping(string messageType, string externalType)
        {
            MessageType = messageType;
            ExternalType = externalType;
        }

        public string MessageType { get; set; }
        public string ExternalType { get; set; }
    }
}
