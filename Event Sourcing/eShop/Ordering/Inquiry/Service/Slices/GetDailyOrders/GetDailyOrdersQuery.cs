using eShop.Ordering.Inquiry.StateViews;
using vesa.Core.Abstractions;

namespace eShop.Ordering.Inquiry.Service.Slices.GetDailyOrders;

public class GetDailyOrdersQuery : IQuery<DailyOrdersStateView>
{
    public GetDailyOrdersQuery(DateTimeOffset stateViewDate)
    {
        StateViewDate = stateViewDate;
    }

    public DateTimeOffset StateViewDate { get; }

    public static bool TryParse(DateTimeOffset stateViewDate, out GetDailyOrdersQuery? query)
    {
        var parsed = false;
        query = new GetDailyOrdersQuery(stateViewDate);
        parsed = true;
        return parsed;
    }
}
