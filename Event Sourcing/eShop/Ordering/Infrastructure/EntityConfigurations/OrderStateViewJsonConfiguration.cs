using eShop.Ordering.Infrastructure.SQL.Entities;
using eShop.Ordering.Inquiry.StateViews;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using vesa.SQL.Infrastructure;

namespace eShop.Ordering.Infrastructure.SQL.EntityConfigurations;

public class OrderStateViewJsonConfiguration : StateViewJsonConfiguration<OrderStateViewJson, OrderStateView>
{
    public override void Configure(EntityTypeBuilder<OrderStateViewJson> builder)
    {
        builder.ToTable(nameof(OrderStateViewJson));
        base.Configure(builder);
    }
}