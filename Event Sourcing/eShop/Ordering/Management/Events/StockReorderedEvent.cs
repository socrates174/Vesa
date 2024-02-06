using eShop.Ordering.Data.Models;
using vesa.Core.Infrastructure;

namespace eShop.Ordering.Management.Events;

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
        Subject = GetDefaultSubject(OrderNumber);
    }

    public static string GetDefaultSubject(string orderNumber) => $"StockReorder_{orderNumber}";

    public string OrderNumber { get; init; }
    public IList<OrderItem> Items { get; init; } = new List<OrderItem>();
}