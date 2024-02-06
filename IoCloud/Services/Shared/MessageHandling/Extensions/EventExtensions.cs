using IoCloud.Shared.Domain;
using AutoMapper;
using IoCloud.Shared.MessageHandling.Abstractions;
using IoCloud.Shared.Messages;

namespace IoCloud.Shared.MessageHandling.Extensions
{
    public static class EventExtensions
    {
        static IList<Type> unhandledEvents = AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.FindUnhandledEvents()).ToList();

        public static bool HasHandler(this IEvent anEvent)
        {
            return !unhandledEvents.Contains(anEvent.GetType());
        }

        public static IEnumerable<TOutboxMessage> ToOutboxMessages<TOutboxMessage>(this IList<IDomainMessage> events, IMapper mapper)
            where TOutboxMessage : Message
        {
            var outboxMessages = new List<TOutboxMessage>();
            foreach (var anEvent in events)
            {
                var outboxMessage = mapper.Map(anEvent, anEvent.GetType(), typeof(TOutboxMessage)) as TOutboxMessage;
                outboxMessages.Add(outboxMessage);
            }
            return outboxMessages;
        }
    }
}
