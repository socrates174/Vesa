using AutoMapper;
using eShop.Ordering.OrderInquiry.Data.Entities;

namespace eShop.Ordering.OrderInquiry.Service.UseCases.ViewOrders
{
    public class ViewOrdersMapping : Profile
    {
        public ViewOrdersMapping()
        {
            CreateMap<Order, ViewOrdersReply>();
        }
    }
}