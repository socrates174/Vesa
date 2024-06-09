using eShop.Ordering.Management.Events;
using eShop.Ordering.Management.Exceptions;
using vesa.Core.Abstractions;

namespace eShop.Ordering.Management.Service.ReorderStock;

public class OutOfStockExceptionHandler : IEventHandler<OutOfStockExceptionEvent>
{
    private readonly ICommandHandler<ReorderStockCommand> _commandHandler;

    public OutOfStockExceptionHandler(ICommandHandler<ReorderStockCommand> commandHandler)
    {
        _commandHandler = commandHandler;
    }

    public async Task HandleAsync(OutOfStockExceptionEvent @event, CancellationToken cancellationToken)
    {
        var outOfStockException = @event.Exception as OutOfStockException;
        var reorderStockCommand = new ReorderStockCommand
        (
            // assign the event Id as the command Id so that when the domain generates an event,
            // it takes the command Id as the IdempotencyToken and will prevent the command from being handled twice
            @event.Id,
            outOfStockException.OrderNumber,
            outOfStockException.Items,
            @event.TriggeredBy,
            @event.SequenceNumber
        );
        var events = await _commandHandler.HandleAsync(reorderStockCommand, new CancellationToken());
    }
}
