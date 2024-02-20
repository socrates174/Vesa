using vesa.Core.Abstractions;

namespace vesa.Core.Infrastructure;

public abstract class Event : IEvent
{
    protected Event()
    {
        EventTypeName = GetType().FullName;
    }

    protected Event(string triggeredBy, string idempotencyToken) : this()
    {
        TriggeredBy = triggeredBy;
        IdempotencyToken = idempotencyToken;
    }

    public string Id { get; init; } = Guid.NewGuid().ToString();
    public string Subject { get; init; }
    public string SubjectPrefix { get; init; }
    public string EventTypeName { get; init; }
    public DateTimeOffset EventDate { get; init; } = DateTimeOffset.Now;
    public string TriggeredBy { get; init; }
    public int SequenceNumber { get; init; } = 0;
    public string IdempotencyToken { get; init; }
}
