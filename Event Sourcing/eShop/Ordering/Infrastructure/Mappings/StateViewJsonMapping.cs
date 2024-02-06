using AutoMapper;
using eShop.Ordering.Infrastructure.SQL.Entities;

namespace eShop.Ordering.Infrastructure.SQL.Mappings;

public class StateViewJsonMapping : Profile
{
    public StateViewJsonMapping() : base()
    {
        CreateMap<OrderStateViewJson, OrderStateViewJson>();
        CreateMap<CustomerOrdersStateViewJson, CustomerOrdersStateViewJson>();
        CreateMap<StatusOrdersStateViewJson, StatusOrdersStateViewJson>();
        CreateMap<DailyOrdersStateViewJson, DailyOrdersStateViewJson>();
    }
}