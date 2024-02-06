using IoCloud.Shared.Settings.Abstractions;

namespace IoCloud.Shared.Settings.Infrastructure
{
    public class InternalMessageMapping : MessageMapping, IInternalMessageMapping
    {
        public InternalMessageMapping() : base()
        {
        }

        public InternalMessageMapping(string messageType, string externalType, string internalType) : base(messageType, externalType)
        {
            InternalType = internalType;
        }

        public string InternalType { get; set; }
    }
}
