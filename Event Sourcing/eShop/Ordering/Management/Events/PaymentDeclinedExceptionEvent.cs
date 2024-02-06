using eShop.Ordering.Management.Exceptions;
using vesa.Core.Infrastructure;

namespace eShop.Ordering.Management.Events;

public class PaymentDeclinedExceptionEvent : ExceptionEvent
{
    public PaymentDeclinedExceptionEvent
    (
        PaymentDeclinedException exception,
        string triggeredBy,
        string idempotencyToken
    )
        : base(exception, triggeredBy, idempotencyToken)
    {
        Subject = $"Order_{exception.OrderNumber}";
    }
}