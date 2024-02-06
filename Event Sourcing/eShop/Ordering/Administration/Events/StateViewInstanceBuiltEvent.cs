using vesa.Core.Infrastructure;

namespace eShop.Ordering.Administration.Events;
public class StateViewInstanceBuiltEvent : Event

{
    public StateViewInstanceBuiltEvent
    (
        string stateViewName,
        string subject,
        string triggeredBy,
        string idempotencyToken
    )
        : base(triggeredBy, idempotencyToken)
    {
        StateViewName = stateViewName;
        Subject = subject;
    }

    public string StateViewName { get; }
    public string Subject { get; }
}
