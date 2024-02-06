using vesa.Core.Infrastructure;
using eShop.Inventory.Data.ValueObjects;

namespace eShop.Inventory.Core.IntegrationEvents;

public class StockReorderedIntegrationEvent : Event
{
    public StockReorderedIntegrationEvent
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
        Subject = GetDefaultSubject(OrderNumber);
    }

    public static string GetDefaultSubject(string orderNumber) => $"StockReorder_{orderNumber}";

    public string OrderNumber { get; init; }
    public IList<OrderItem> Items { get; init; } = new List<OrderItem>();
}