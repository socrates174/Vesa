using vesa.Core.Infrastructure;
using eShop.Inventory.Data.ValueObjects;

namespace eShop.Inventory.Core.Events;

public class StockReorderedEvent : Event
{
    public StockReorderedEvent
    (
        string orderNumber,
        IEnumerable<OrderItem> items,
        string triggeredBy,
        string idempotencyToken
    )
        : base(triggeredBy, idempotencyToken)
    {
        OrderNumber = orderNumber;
        foreach (var item in items)
        {
            Items.Add(item);
        }
        Subject = $"StockReorder_{OrderNumber}";
    }

    public string OrderNumber { get; init; }
    public IList<OrderItem> Items { get; init; } = new List<OrderItem>();
}